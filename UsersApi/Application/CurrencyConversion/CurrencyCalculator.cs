namespace UsersApi.Application.CurrencyConversion;

// calculo de la conversion separado del handler para poder probarlo sin base de datos
public static class CurrencyCalculator
{
    public static decimal Convert(decimal amount, decimal fromRateToBase, decimal toRateToBase)
    {
        // primero se pasa el monto a la moneda base y despues a la moneda de destino
        var montoBase = amount * fromRateToBase;
        return Math.Round(montoBase / toRateToBase, 4);
    }
}
