namespace StarnetCosmo.Core.Entities;

public enum PipelineStage
{
    Nuevo = 0,
    Contactado = 1,
    Calificado = 2,
    DemoAgendada = 3,
    DemoRealizada = 4,
    Cotizacion = 5,
    Negociacion = 6,
    Ganado = 7,
    Perdido = 8,
    NoCalifica = 9
}

public enum LeadSource
{
    GoogleMaps = 0,
    Facebook = 1,
    Instagram = 2,
    Referido = 3,
    Inbound = 4,
    WebScraper = 5,
    Manual = 6
}

public enum ProductInterest
{
    EkklesiApp = 0,
    ChordSync = 1,
    SermonSync = 2,
    SuiteCompleta = 3
}

public enum InteractionType
{
    WhatsApp = 0,
    Llamada = 1,
    Demo = 2,
    Correo = 3,
    NotaObjecion = 4,
    Otro = 5
}
