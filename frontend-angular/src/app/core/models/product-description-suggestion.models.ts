export interface SuggestProductDescriptionRequest {
  code: string;
  partialDescription?: string;
}

export interface SuggestProductDescriptionResponse {
  suggestedDescription: string;
}
