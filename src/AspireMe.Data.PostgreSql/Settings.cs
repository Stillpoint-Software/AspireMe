﻿namespace AspireMe.Data.PostgreSql;
public class AuthSettings
{
    public string? Domain { get; set; }
    public string? Audience { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
}