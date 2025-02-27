﻿using CantinaIBJ.WebApi.Models.Read.Core;
using CantinaIBJ.WebApi.Models.Read.Order;

namespace CantinaIBJ.WebApi.Models.Read.Customer;

public class CustomerPersonReadModel : BaseReadModel
{
    /// <summary>
    /// Nome do cliente
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// E-mail do cliente
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Celular do cliente
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Valor do saldo existente na conta do cliente
    /// </summary>
    public decimal? Balance { get; set; }
    
    public List<OrderReadModel>? Orders { get; set; }
}