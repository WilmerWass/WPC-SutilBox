using System;
using System.Diagnostics;

namespace Wpc_SutilBox.Models
{
    public class ProcessInfoDto
    {
        public int Pid { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public ProcessPriorityClass Priority { get; set; }
        public double WorkingSetMb { get; set; }
        public DateTime? StartTime { get; set; }
        public bool IsForeground { get; set; }

        // Propiedad para marcar procesos protegidos/crÃ­ticos del sistema
        public bool IsCritical { get; set; }

        // Propiedades dinÃ¡micas para enlace con la UI
        public string ActionButtonText => IsCritical ? "Revisar" : "Finalizar Tarea";
        public bool CanKill => !IsCritical;
    }

    public class ProcessImpactStats
    {
        public int ProcessCount { get; set; }
        public double TotalWorkingSetMb { get; set; }
    }
}

