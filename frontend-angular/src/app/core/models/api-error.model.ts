export interface ApiErrorPayload {
  message?: string;
  traceId?: string;
  errorCode?: string | null;
}
