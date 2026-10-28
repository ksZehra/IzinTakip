namespace IzinTakip.Infrastructure.Email;

public sealed class KaliciEmailHatasiException(string message, Exception? innerException = null) : Exception(message, innerException);
