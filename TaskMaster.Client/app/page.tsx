import { checkServerHealth, getSystemInfo } from "@/lib/actions/health"
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Activity, Server, AlertCircle } from "lucide-react"

export default async function Home() {
  // Ahora las llamadas no lanzan excepciones, devuelven un objeto seguro
  const healthRes = await checkServerHealth()
  
  // Lanzamos el error explícitamente para que Next.js renderice app/error.tsx
  if (!healthRes.success) {
    throw new Error(healthRes.error || "Error de conexión con el servidor");
  }

  const infoRes = await getSystemInfo()

  const isHealthy = healthRes.success && healthRes.data === "Healthy";
  const systemVersion = infoRes.success ? infoRes.data.version : "Desconocida";

  return (
    <main className="flex min-h-screen items-center justify-center bg-slate-50 p-6 dark:bg-slate-950">
      <Card className="w-full max-w-md shadow-lg">
        <CardHeader className="text-center">
          <CardTitle className="text-3xl font-extrabold tracking-tight">
            TaskMaster
          </CardTitle>
          <CardDescription>
            Infraestructura Enterprise Conectada
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          
          {/* Status del API */}
          <div className="flex items-center justify-between rounded-lg border bg-white p-3 dark:bg-slate-900">
            <div className="flex items-center gap-2 text-sm font-medium">
              {isHealthy ? (
                <Activity className="h-4 w-4 text-emerald-500" />
              ) : (
                <AlertCircle className="h-4 w-4 text-red-500" />
              )}
              Estado del Servidor
            </div>
            <Badge
              variant="outline"
              className={isHealthy ? "border-emerald-500 text-emerald-600" : "border-red-500 text-red-600"}
            >
              {isHealthy ? healthRes.data : "Offline"}
            </Badge>
          </div>

          {/* Info del Sistema */}
          <div className="flex items-center justify-between rounded-lg border bg-white p-3 dark:bg-slate-900">
            <div className="flex items-center gap-2 text-sm font-medium">
              <Server className="h-4 w-4 text-blue-500" />
              Versión del Core
            </div>
            <span className="font-mono text-xs text-slate-500">
              {systemVersion}
            </span>
          </div>

          {/* Mostrar detalle del error si falla la conexión */}
          {(!healthRes.success || !infoRes.success) && (
            <div className="mt-4 text-xs text-red-500 bg-red-50 dark:bg-red-950/20 p-2 rounded border border-red-100 dark:border-red-900">
              {healthRes.error || infoRes.error}
            </div>
          )}

        </CardContent>
      </Card>
    </main>
  )
}
