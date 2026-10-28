namespace IzinTakip.Gateway.Bff;

public sealed record GirisRequest(string Eposta, string Parola);

public sealed record ApiGirisResponse(string AccessToken, DateTime SureBitis);
