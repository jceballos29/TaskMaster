export interface Result<T> {
    isSuccess: boolean;
    value: T;
    errorCode: string;
    errorMessage: string;
}

export interface ApiError {
  error: string;
  error_description: string;
  status: number;
}