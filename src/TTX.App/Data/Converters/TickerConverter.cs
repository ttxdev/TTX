using TTX.Domain.ValueObjects;

namespace TTX.App.Data.Converters;

public class TickerConverter() : ValueObjectConverter<Ticker, string>(Ticker.Create);
