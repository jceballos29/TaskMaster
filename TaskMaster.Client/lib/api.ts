import { ApiResult } from "@/types"

export function getApiUrl(endpoint: string): string {
  // 1. Aseguramos que el endpoint empiece con '/'
  const path = endpoint.startsWith("/") ? endpoint : `/${endpoint}`

  // 2. Normalizamos la ruta para que siempre incluya el prefijo '/api'
  const basePath = path.startsWith("/api") ? path : `/api${path}`

  if (typeof window === "undefined") {
    // Si estamos en el servidor (SSR), usamos la URL interna de Kestrel
    const internalUrl = process.env.API_INTERNAL_URL || "http://localhost:5000"
    return `${internalUrl}${basePath}` // Resultado: http://localhost:5000/api/health
  }

  // Si estamos en el cliente, usamos la ruta pública interceptable por el proxy
  return basePath // Resultado: /api/health
}

export async function fetchFromApi<T>(endpoint: string, options?: RequestInit): Promise<ApiResult<T>> {
  const url = getApiUrl(endpoint)

  try {
    const response = await fetch(url, {
      cache: "no-store",
      ...options,
    })

    if (!response.ok) {
      return { success: false, data: null, error: `API Error: ${response.status} ${response.statusText}` };
    }

    // Intentamos parsear como JSON, si falla devolvemos texto
    const contentType = response.headers.get("content-type");
    if (contentType && contentType.includes("application/json")) {
      const data = await response.json();
      return { success: true, data: data as T, error: null };
    } else {
      const text = await response.text();
      return { success: true, data: text as unknown as T, error: null };
    }
  } catch (error) {
    // Al atrapar el error y devolver un objeto, evitamos el crash y el bucle de Next.js
    return { success: false, data: null, error: `Failed to connect to API: ${(error as Error).message}` };
  }
}
