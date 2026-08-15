using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpc_SutilBox.ViewModels;

#nullable enable

namespace Wpc_SutilBox.Core
{
    /// <summary>
    /// Genera el informe de diagnóstico del módulo "Revisar mi PC" en formato Markdown.
    /// Salida canónica: ANOSUBIR/BETA_1/1.2_PC_REVIEW/RESULTADOS.md
    /// </summary>
    public static class ReportGenerator
    {
        /// <summary>
        /// Escribe el informe de diagnóstico actual en <paramref name="outputPath"/>.
        /// Crea los directorios intermedios si no existen.
        /// </summary>
        /// <param name="vm">ViewModel ya cargado con datos.</param>
        /// <param name="outputPath">Ruta absoluta del archivo de salida.</param>
        public static async Task GenerateAsync(PcReviewViewModel vm, string outputPath)
        {
            if (vm == null) throw new ArgumentNullException(nameof(vm));
            if (string.IsNullOrWhiteSpace(outputPath)) throw new ArgumentException("La ruta no puede estar vacía.", nameof(outputPath));

            var dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            var sb = new StringBuilder();

            sb.AppendLine("# 🖥️ WPC-SutilBox — Diagnóstico \"Revisar mi PC\"");
            sb.AppendLine($"> **Generado:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}  ");
            sb.AppendLine($"> **Equipo:** {Environment.MachineName}");
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();

            // ── 1. Métricas de Rendimiento ────────────────────────────────────────
            sb.AppendLine("## 1. Métricas de Rendimiento (en vivo)");
            sb.AppendLine();
            sb.AppendLine($"| Métrica     | Valor         |");
            sb.AppendLine($"|-------------|---------------|");
            sb.AppendLine($"| CPU         | {vm.CpuUsage:F1} %       |");
            sb.AppendLine($"| RAM         | {vm.RamUsage:F1} %       |");
            sb.AppendLine($"| Temp. CPU   | {(vm.CpuTempC > 0 ? $"{vm.CpuTempC:F0} °C" : "No disponible")} |");
            sb.AppendLine();

            // ── 2. Información del Sistema ─────────────────────────────────────────
            sb.AppendLine("## 2. Información del Sistema");
            sb.AppendLine();
            if (vm.SystemInfo != null)
            {
                sb.AppendLine($"| Campo       | Valor                          |");
                sb.AppendLine($"|-------------|--------------------------------|");
                foreach (var kv in vm.SystemInfo.GeneralItems)
                    sb.AppendLine($"| {kv.Key,-11} | {kv.Value,-30} |");
            }
            else
            {
                sb.AppendLine("> No se pudo obtener la información del sistema.");
            }
            sb.AppendLine();

            // ── 3. Salud de Discos (SMART) ────────────────────────────────────────
            sb.AppendLine("## 3. Salud de Discos (SMART)");
            sb.AppendLine();
            if (vm.Disks.Count == 0)
            {
                sb.AppendLine("> No se encontraron discos o no se pudo leer SMART.");
            }
            else
            {
                sb.AppendLine("| Disco | Modelo | Capacidad | Estado SMART | Temperatura |");
                sb.AppendLine("|-------|--------|-----------|--------------|-------------|");
                foreach (var d in vm.Disks)
                {
                    string smart  = d.SmartStatusKnown ? (d.SmartOk ? "✅ OK" : "❌ FALLO") : "⚠️ Desconocido";
                    string temp   = d.Temperature > 0 ? $"{d.Temperature} °C" : "—";
                    sb.AppendLine($"| {d.DeviceId,-5} | {d.Model,-20} | {d.Capacity,-9} | {smart,-12} | {temp,-11} |");
                }
            }
            sb.AppendLine();

            // ── 4. Drivers con Problemas ──────────────────────────────────────────
            sb.AppendLine("## 4. Drivers con Problemas");
            sb.AppendLine();
            if (vm.DriverIssues.Count == 0)
            {
                sb.AppendLine("> ✅ No se detectaron drivers con problemas.");
            }
            else
            {
                sb.AppendLine($"> ⚠️ Se encontraron **{vm.DriverIssues.Count}** driver(s) con problemas.");
                sb.AppendLine();
                sb.AppendLine("| Driver | Estado | Error |");
                sb.AppendLine("|--------|--------|-------|");
                foreach (var d in vm.DriverIssues)
                    sb.AppendLine($"| {d.Name} | {d.Status} | {d.ErrorDescription} |");
            }
            sb.AppendLine();

            // ── 5. Procesos de Alto Consumo ───────────────────────────────────────
            sb.AppendLine("## 5. Procesos de Alto Consumo (Top 15)");
            sb.AppendLine();
            var top15 = vm.Processes.Take(15).ToList();
            if (top15.Count == 0)
            {
                sb.AppendLine("> No hay datos de procesos disponibles.");
            }
            else
            {
                sb.AppendLine("| # | Proceso | RAM (MB) | PID | Crítico |");
                sb.AppendLine("|---|---------|----------|-----|---------|");
                int i = 1;
                foreach (var p in top15)
                    sb.AppendLine($"| {i++} | {p.Name} | {p.WorkingSetMb:F1} | {p.Pid} | {(p.IsCritical ? "Sí" : "No")} |");
            }
            sb.AppendLine();

            // ── 6. Resumen de Salud ───────────────────────────────────────────────
            sb.AppendLine("## 6. Resumen de Salud");
            sb.AppendLine();
            bool cpuHigh    = vm.CpuUsage > 85;
            bool ramHigh    = vm.RamUsage > 85;
            bool diskFail   = vm.Disks.Any(d => d.SmartStatusKnown && !d.SmartOk);
            bool driverFail = vm.DriverIssues.Count > 0;

            sb.AppendLine($"- CPU:         {(cpuHigh   ? "⚠️ Alto consumo"    : "✅ Normal")}");
            sb.AppendLine($"- RAM:         {(ramHigh   ? "⚠️ Alto consumo"    : "✅ Normal")}");
            sb.AppendLine($"- Discos:      {(diskFail  ? "❌ Fallo SMART"     : "✅ Saludables")}");
            sb.AppendLine($"- Drivers:     {(driverFail? "⚠️ Problemas detectados" : "✅ Sin problemas")}");
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine("*Informe generado por WPC-SutilBox — Beta 1.2*");

            await File.WriteAllTextAsync(outputPath, sb.ToString(), Encoding.UTF8);
        }
    }
}
