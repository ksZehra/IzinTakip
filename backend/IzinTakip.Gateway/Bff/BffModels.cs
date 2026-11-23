namespace IzinTakip.Gateway.Bff;

public sealed record LoginRequest(string Eposta, string Parola);

public sealed record ApiLoginResponse(string AccessToken, DateTime SureBitis);
