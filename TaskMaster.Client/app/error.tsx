// src/app/error.tsx
"use client" // Los componentes de error deben ser de cliente por diseño en Next.js

import {
  Card,
  CardAction,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"
import { useEffect } from "react"

// Next.js inyecta el error y una función para reintentar
export default function Error({
  error,
  reset,
}: {
  error: Error & { digest?: string }
  reset: () => void
}) {
  useEffect(() => {
    // Aquí podrías enviar el error a un servicio como Sentry
    console.error("Critical API Failure:", error)
  }, [error])

  return (
    <main className="flex min-h-screen flex-col items-center justify-center bg-red-50 p-24">
      <Card className="w-full md:max-w-md">
        <CardHeader>
          <CardTitle className="text-2xl font-bold text-red-600">
            Servicio no disponible
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="mb-6 text-gray-600">No pudimos conectar con el servidor principal. Es posible que la API
            esté caída o en mantenimiento.</div>
          <div className="mb-6 overflow-auto rounded bg-gray-100 p-4 text-left font-mono text-sm text-gray-800">
            {error.message}
          </div>
        </CardContent>
        <CardFooter>
          <button
            onClick={() => reset()}
            className="w-full rounded bg-red-600 px-4 py-2 font-bold text-white transition-colors hover:bg-red-700"
          >
            Reintentar conexión
          </button>
        </CardFooter>
      </Card>
    </main>
  )
}
