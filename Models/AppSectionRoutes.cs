using System.Collections.Generic;

namespace Wpc_SutilBox.Models;

/// <summary>Resolves legacy section identifiers to their canonical destination.</summary>
public static class AppSectionRoutes
{
    private static readonly IReadOnlyDictionary<AppSection, AppSection> Aliases =
        new Dictionary<AppSection, AppSection>
        {
            [AppSection.ProgramasInstalar] = AppSection.ProgramasActualizar,
            [AppSection.Bloatware] = AppSection.Desinstalador,
            [AppSection.Red] = AppSection.TestRed,
            [AppSection.Hardware] = AppSection.RevisarPC,
            [AppSection.Escala] = AppSection.Configuracion
        };

    public static AppSection Canonicalize(AppSection section) =>
        Aliases.TryGetValue(section, out var destination) ? destination : section;
}
