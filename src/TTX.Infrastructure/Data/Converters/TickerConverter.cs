using TTX.Domain.ValueObjects;

namespace TTX.Infrastructure.Data.Converters;

public class TickerConverter() : ValueObjectConverter<Ticker, string>(Ticker.Create);
