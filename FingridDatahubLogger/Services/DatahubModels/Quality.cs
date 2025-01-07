// ReSharper disable InconsistentNaming
namespace FingridDatahubLogger.Services.DatahubModels;

public enum Quality
{
    NONE,       // Ei mitään
    MISSING,    // Puuttuva
    UNCERTAIN,  // Epävarma
    EST,        // Arvioitu
    CALC,       // Laskettu
    OK,         // OK
    REVISED,    // Muutettu
    PMISSING,   // Osittain puuttuva
    E21,        // Väliaikainen
    E56         // Arvioitu, hyväksytään laskutukseen
}