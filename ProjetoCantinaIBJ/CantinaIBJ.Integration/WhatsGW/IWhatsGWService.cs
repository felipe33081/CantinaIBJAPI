using CantinaIBJ.Integration.WhatsGW.Models.Response;

namespace CantinaIBJ.Integration.WhatsGW;

public interface IWhatsGWService
{
    Task<WhatsGWSendMessageResponse?> WhatsSendMessage(string toNumber, string message);
}