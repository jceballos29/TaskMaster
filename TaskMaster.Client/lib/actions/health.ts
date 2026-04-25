"use server"

import { fetchFromApi } from "@/lib/api"
import { SystemInfo } from "@/types/system-info" // Asumiendo que tienes esta interfaz

export async function checkServerHealth() {
  return await fetchFromApi<string>("/health");
}

export async function getSystemInfo() {
  return await fetchFromApi<SystemInfo>("/system/info");
}
