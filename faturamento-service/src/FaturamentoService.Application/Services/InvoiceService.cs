using FaturamentoService.Application.DTOs.Requests;
using FaturamentoService.Application.DTOs.Responses;
using FaturamentoService.Application.Exceptions;
using FaturamentoService.Application.Interfaces;
using FaturamentoService.Application.Mapping;
using FaturamentoService.Domain.Entities;
using FaturamentoService.Domain.Enums;
using FaturamentoService.Domain.Exceptions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using AppValidationException = FaturamentoService.Application.Exceptions.ValidationException;

namespace FaturamentoService.Application.Services;

public class InvoiceService : IInvoiceService
{
    private const int StatusOk = 200;
    private const int StatusBadRequest = 400;
    private const int StatusNotFound = 404;
    private const int StatusConflict = 409;
    private const int StatusUnprocessableEntity = 422;
    private const int StatusServiceUnavailable = 503;
    private const int StatusInternalServerError = 500;

    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IStockServiceClient _stockServiceClient;
    private readonly IValidator<CreateInvoiceRequestDto> _createValidator;
    private readonly IValidator<AddInvoiceItemsRequestDto> _addItemsValidator;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(
        IInvoiceRepository invoiceRepository,
        IStockServiceClient stockServiceClient,
        IValidator<CreateInvoiceRequestDto> createValidator,
        IValidator<AddInvoiceItemsRequestDto> addItemsValidator,
        ILogger<InvoiceService> logger)
    {
        _invoiceRepository = invoiceRepository;
        _stockServiceClient = stockServiceClient;
        _createValidator = createValidator;
        _addItemsValidator = addItemsValidator;
        _logger = logger;
    }

    public async Task<InvoiceResponseDto> CreateAsync(CreateInvoiceRequestDto request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_createValidator, request, cancellationToken);

        var invoice = Invoice.Create();
        await _invoiceRepository.AddAsync(invoice, cancellationToken);
        await _invoiceRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Invoice {InvoiceId} created with sequential number {SequentialNumber}", invoice.Id, invoice.SequentialNumber);

        return invoice.ToResponse();
    }

    public async Task<List<InvoiceResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var invoices = await _invoiceRepository.GetAllAsync(cancellationToken);
        return invoices.Select(x => x.ToResponse()).ToList();
    }

    public async Task<InvoiceResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await GetExistingInvoiceAsync(id, cancellationToken);
        return invoice.ToResponse();
    }

    public async Task<InvoiceResponseDto> GetBySequentialNumberAsync(int sequentialNumber, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetBySequentialNumberAsync(sequentialNumber, cancellationToken);
        if (invoice is null)
        {
            throw new NotFoundException($"Invoice with sequential number '{sequentialNumber}' was not found.");
        }

        return invoice.ToResponse();
    }

    public async Task<InvoiceResponseDto> AddItemsAsync(Guid invoiceId, AddInvoiceItemsRequestDto request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_addItemsValidator, request, cancellationToken);

        var invoice = await GetExistingInvoiceAsync(invoiceId, cancellationToken);
        if (invoice.Status != InvoiceStatus.Open)
        {
            throw new ConflictException("Items can only be added to OPEN invoices.");
        }

        foreach (var item in request.Items)
        {
            invoice.AddItem(item.ProductCode, item.ProductDescription, item.Quantity);
        }

        await _invoiceRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Added {ItemsCount} items to invoice {InvoiceId}", request.Items.Count, invoice.Id);

        return invoice.ToResponse();
    }

    public async Task<PrintInvoiceResponseDto> PrintAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await GetExistingInvoiceAsync(invoiceId, cancellationToken);
        if (invoice.Status != InvoiceStatus.Open)
        {
            throw new ConflictException("Only OPEN invoices can be printed.");
        }

        if (!invoice.Items.Any())
        {
            throw new ConflictException("Cannot print an invoice without items.");
        }

        invoice.RegisterPrintAttempt();
        await _invoiceRepository.SaveChangesAsync(cancellationToken);

        try
        {
            var groupedItems = invoice.Items
                .GroupBy(x => x.ProductCode)
                .Select(group => (ProductCode: group.Key, Quantity: group.Sum(item => item.Quantity)))
                .ToList();

            var validationResult = await _stockServiceClient.ValidateStockAsync(groupedItems, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errorMessage = string.Join(" | ", validationResult.Items
                    .Where(x => !x.IsAvailable)
                    .Select(x => $"{x.ProductCode}: {x.Message}"));

                throw new AppValidationException($"Unable to print invoice due to stock validation failure. {errorMessage}");
            }

            await _stockServiceClient.DebitStockAsync(groupedItems, cancellationToken);

            invoice.ClearPrintError();
            invoice.Close();
            await _invoiceRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Invoice {InvoiceId} printed successfully", invoice.Id);

            return new PrintInvoiceResponseDto
            {
                Success = true,
                Message = "Invoice printed successfully.",
                Invoice = invoice.ToResponse()
            };
        }
        catch (Exception exception) when (exception is AppValidationException or ExternalServiceException)
        {
            invoice.RegisterPrintFailure(exception.Message);
            await _invoiceRepository.SaveChangesAsync(cancellationToken);

            _logger.LogWarning(exception, "Invoice {InvoiceId} print failed", invoice.Id);
            throw;
        }
    }

    public async Task<IdempotentPrintResponseDto> PrintIdempotentAsync(
        Guid invoiceId,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        var normalizedKey = idempotencyKey.Trim();

        var existingRecord = await _invoiceRepository.GetPrintIdempotencyRecordAsync(invoiceId, normalizedKey, cancellationToken);
        if (existingRecord?.IsCompleted == true)
        {
            return ToIdempotentResponse(existingRecord);
        }

        await using var transaction = await _invoiceRepository.BeginTransactionAsync(cancellationToken);

        existingRecord = await _invoiceRepository.GetPrintIdempotencyRecordAsync(invoiceId, normalizedKey, cancellationToken);
        if (existingRecord?.IsCompleted == true)
        {
            return ToIdempotentResponse(existingRecord);
        }

        var record = InvoicePrintIdempotencyRecord.Create(invoiceId, normalizedKey);
        var created = await _invoiceRepository.TryCreatePrintIdempotencyRecordAsync(record, cancellationToken);

        if (!created)
        {
            existingRecord = await _invoiceRepository.GetPrintIdempotencyRecordAsync(invoiceId, normalizedKey, cancellationToken)
                ?? throw new ConflictException("Unable to recover the previous print result for the provided idempotency key.");

            return ToIdempotentResponse(existingRecord);
        }

        IdempotentPrintResponseDto result;

        try
        {
            var printResponse = await PrintAsync(invoiceId, cancellationToken);
            result = CreateSuccessResponse(printResponse);
        }
        catch (Exception exception) when (exception is NotFoundException
                                         or ConflictException
                                         or AppValidationException
                                         or DomainException
                                         or ExternalServiceException)
        {
            result = CreateErrorResponse(exception);
        }

        record.Complete(result.StatusCode, result.ResponseJson);
        await _invoiceRepository.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return result;
    }

    private async Task<Invoice> GetExistingInvoiceAsync(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, cancellationToken);
        if (invoice is null)
        {
            throw new NotFoundException($"Invoice with id '{id}' was not found.");
        }

        return invoice;
    }

    private static async Task ValidateAsync<T>(IValidator<T> validator, T instance, CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(instance, cancellationToken);
        if (!result.IsValid)
        {
            var message = string.Join("; ", result.Errors.Select(x => x.ErrorMessage));
            throw new AppValidationException(message);
        }
    }

    private static IdempotentPrintResponseDto ToIdempotentResponse(InvoicePrintIdempotencyRecord record)
    {
        return new IdempotentPrintResponseDto
        {
            StatusCode = record.ResponseStatusCode,
            ResponseJson = record.ResponseJson
        };
    }

    private static IdempotentPrintResponseDto CreateSuccessResponse(PrintInvoiceResponseDto response)
    {
        return new IdempotentPrintResponseDto
        {
            StatusCode = StatusOk,
            ResponseJson = JsonSerializer.Serialize(response)
        };
    }

    private static IdempotentPrintResponseDto CreateErrorResponse(Exception exception)
    {
        var statusCode = exception switch
        {
            NotFoundException => StatusNotFound,
            ConflictException => StatusConflict,
            AppValidationException => StatusUnprocessableEntity,
            DomainException => StatusBadRequest,
            ExternalServiceException => StatusServiceUnavailable,
            _ => StatusInternalServerError
        };

        var message = statusCode switch
        {
            StatusInternalServerError => "Ocorreu um erro inesperado.",
            StatusServiceUnavailable => "Nao foi possivel emitir a nota fiscal agora. Tente novamente mais tarde.",
            _ => exception.Message
        };

        return new IdempotentPrintResponseDto
        {
            StatusCode = statusCode,
            ResponseJson = JsonSerializer.Serialize(new ErrorResponseDto
            {
                Message = message
            })
        };
    }
}
