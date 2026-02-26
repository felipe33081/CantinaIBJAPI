using AutoMapper;
using CantinaIBJ.Data.Contracts;
using CantinaIBJ.Data.Contracts.Customer;
using CantinaIBJ.Integration.WhatsGW;
using CantinaIBJ.Model;
using CantinaIBJ.Model.Customer;
using CantinaIBJ.Model.Enumerations;
using CantinaIBJ.Model.Interfaces;
using CantinaIBJ.Model.Orders;
using CantinaIBJ.WebApi.Common;
using CantinaIBJ.WebApi.Controllers.Core;
using CantinaIBJ.WebApi.Helpers;
using CantinaIBJ.WebApi.Models;
using CantinaIBJ.WebApi.Models.Create.Order;
using CantinaIBJ.WebApi.Models.Read.Order;
using CantinaIBJ.WebApi.Models.Update.Order;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace CantinaIBJ.WebApi.Controllers;

public class OrderController : CoreController
{
    readonly IOrderRepository _orderRepository;
    readonly IProductRepository _productRepository;
    readonly ICustomerPersonRepository _customerPersonRepository;
    readonly IWhatsGWService _whatsGWService;
    readonly IPrinterService _printerService;
    readonly OrderHelper _orderHelper;
    readonly IMapper _mapper;
    readonly ILogger<OrderController> _logger;

    public OrderController(
        IMapper mapper,
        ILogger<OrderController> logger,
        IOrderRepository orderRepository,
        ICustomerPersonRepository customerPersonRepository,
        OrderHelper orderHelper,
        IProductRepository productRepository,
        IWhatsGWService whatsGWService,
        IPrinterService printerService)
    {
        _mapper = mapper;
        _logger = logger;
        _orderRepository = orderRepository;
        _customerPersonRepository = customerPersonRepository;
        _orderHelper = orderHelper;
        _productRepository = productRepository;
        _whatsGWService = whatsGWService;
        _printerService = printerService;
    }

    /// <summary>
    /// Lista todos os pedidos
    /// </summary>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <param name="searchString"></param>
    /// <param name="id"></param>
    /// <param name="isDeleted"></param>
    /// <param name="orderBy"></param>
    /// /// <param name="status"></param>
    /// <response code="200">Sucesso</response>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
    [HttpGet]
    [ProducesResponseType(typeof(ListDataPagination<OrderReadModel>), 200)]
    public async Task<IActionResult> ListAsync(
        [FromQuery] int page = 0,
        [FromQuery] int size = 10,
        [FromQuery] string? searchString = null,
        [FromQuery] int? id = null,
        [FromQuery] bool isDeleted = false,
        [FromQuery] string? orderBy = null,
        [FromQuery] OrderStatus? status = null)
    {
        try
        {
            var orders = await _orderRepository.GetListOrders(page, size, searchString, id, isDeleted, orderBy, status);

            var newData = new ListDataPagination<OrderReadModel>()
            {
                Data = orders.Data.Select(c => _mapper.Map<OrderReadModel>(c)).ToList(),
                Page = page,
                TotalItems = orders.TotalItems,
                TotalPages = orders.TotalPages
            };

            Response.Headers.Add("X-Total-Count", orders.TotalItems.ToString());

            return Ok(newData);
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }

    /// <summary>
    /// Acessa um registro de pedido por Id(Código)
    /// </summary>
    /// <param name="id">Id do pedido</param>
    /// <response code="200">Sucesso</response>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
    /// <response code="404">Chave não encontrada</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderReadModel), 200)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        try
        {
            var order = await _orderRepository.GetOrderByIdEndpointAsync(id);
            if (order == null)
                return NotFound(new { errors = "Pedido não encontrado" });

            var readOrder = _mapper.Map<OrderReadModel>(order);

            return Ok(readOrder);
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }

    /// <summary>
    /// Cria um novo registro de um pedido
    /// </summary>
    /// <param name="model">Modelo de dados de entrada</param>
    /// <response code="201">Sucesso</response>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
    /// <response code="404">Chave não encontrada</response>
    [HttpPost]
    [ProducesResponseType(typeof(int), 201)]
    public async Task<IActionResult> Create([FromBody] OrderCreateModel model)
    {
        if (!ModelState.IsValid)
            return NotFound(new { errors = "Modelo não é válido" });

        try
        {
            //validação para ver se foi preenchido id de um cliente pré-cadastrado, ou se preencheu o nome do cliente, ou um ou outro, dar exceção se nao preencher nenhum
            if (model.CustomerPersonId == null && string.IsNullOrEmpty(model.CustomerName))
                return BadRequest(new { errors = "Informar um cliente Pré-Cadastrado, Se não cadastro, informar somente o nome do cliente" });

            if (model.Products is null || model.Products.Count <= 0)
                return BadRequest(new { errors = "Produto é obrigatório" });

            var order = _mapper.Map<Order>(model);

            if (model.CustomerPersonId != null && model.CustomerPersonId > 0)
            {
                var customerPerson = await _customerPersonRepository.GetCustomerPersonByIdAsync(model.CustomerPersonId.Value);
                if (customerPerson == null)
                    return NotFound(new { errors = "Cliente não encontrado" });
            }

            //faz a soma do preço dos itens com a quantidade, e atualizar no valor total do pedido
            decimal productsValues = 0;
            foreach (var orderProduct in order.Products)
            {
                var product = await _productRepository.GetProductByIdAsync(orderProduct.ProductId);
                if (product is not null)
                {
                    if (product.Quantity <= 0)
                    {
                        if (product.Disponibility)
                        {
                            product.Disponibility = false;
                            var responseTesteView = await _productRepository.UpdateAsync(product);
                        }
                        return BadRequest(new { errors = $"Não é possível adicionar o produto: {product.Name}, o mesmo não se encontra disponível" });
                    }

                    if (orderProduct.Quantity > product.Quantity)
                        return BadRequest(new { errors = $"Não é possível adicionar uma quantidade maior do que a quantidade em estoque do produto: {product.Name}" });

                    if (orderProduct.Quantity <= 0)
                        return BadRequest(new { errors = $"Não é possível adicionar uma quantidade igual ou menor que zero deste produto" });

                    product.Quantity -= orderProduct.Quantity;
                    _productRepository.UpdateNoCommit(product);

                    orderProduct.Price = product.Price;
                    var totalPriceProduct = orderProduct.Quantity * product.Price;
                    productsValues += totalPriceProduct;
                }
                else
                    return BadRequest(new { errors = "Produto não encontrado" });
            }

            order.TotalValue = productsValues;

            await _orderRepository.AddOrderAsync(order);
            await _productRepository.SaveChangesAsync();

            return StatusCode(201, order.Id);
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }

    /// <summary>
    /// Atualiza um registro de um pedido
    /// </summary>
    /// <param name="id">Id do pedido</param>
    /// <param name="updateModel">Modelo de dados de entrada</param>
    /// <response code="204">Sucesso</response>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
    /// <response code="404">Chave não encontrada</response>
    [HttpPut("{id}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] OrderUpdateModel updateModel)
    {
        if (!ModelState.IsValid)
            return NotFound(new { errors = "Modelo não é válido" });

        try
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound(new { errors = "Pedido não encontrado" });

            if (updateModel.Products is null || updateModel.Products.Count <= 0)
                return BadRequest(new { errors = "Produto é obrigatório" });

            if (order.Status != OrderStatus.InProgress)
                return BadRequest(new { errors = "O pedido não está em andamento, não é possível atualizar" });

            if (updateModel.CustomerPersonId != null && updateModel.CustomerPersonId > 0)
            {
                var customerPerson = await _customerPersonRepository.GetCustomerPersonByIdAsync(updateModel.CustomerPersonId.Value);
                if (customerPerson == null)
                    return NotFound(new { errors = "Cliente não encontrado" });
            }

            // Atualiza a quantidade de produtos removidos
            foreach (var existingItem in order.Products)
            {
                var updatedItem = updateModel.Products.FirstOrDefault(i => i.ProductId == existingItem.ProductId);
                if (updatedItem == null)
                {
                    var product = await _productRepository.GetProductByIdAsync(existingItem.ProductId);
                    product.Quantity += existingItem.Quantity;
                    _productRepository.UpdateNoCommit(product);
                }
            }

            // Atualiza a quantidade de novos produtos adicionados
            decimal productsValues = 0;
            List<KeyValue> keyValues = new();
            foreach (var newItem in updateModel.Products)
            {
                decimal totalPriceProduct = 0;
                var existingItem = order.Products.FirstOrDefault(i => i.ProductId == newItem.ProductId);
                if (existingItem == null)
                {
                    var product = await _productRepository.GetProductByIdAsync(newItem.ProductId);
                    if (product is not null)
                    {
                        if (product.Quantity <= 0)
                        {
                            if (product.Disponibility)
                            {
                                product.Disponibility = false;
                                var responseTesteView = await _productRepository.UpdateAsync(product);
                            }

                            return BadRequest(new { errors = $"Não é possível adicionar o produto: {product.Name}, o mesmo não se encontra disponível" });
                        }

                        if (newItem.Quantity > product.Quantity)
                            return BadRequest(new { errors = $"Não é possível adicionar uma quantidade maior do que a quantidade em estoque do produto: {product.Name}" });

                        if (newItem.Quantity <= 0)
                            return BadRequest(new { errors = $"Não é possível adicionar uma quantidade igual ou menor que zero deste produto" });

                        totalPriceProduct = newItem.Quantity * product.Price;
                        productsValues += totalPriceProduct;
                    }
                    else
                        return BadRequest(new { errors = "Produto não encontrado" });

                    product.Quantity -= newItem.Quantity;
                    KeyValue keyValue = new() { Key = product.Id, Value = product.Price };
                    keyValues.Add(keyValue);
                    _productRepository.UpdateNoCommit(product);
                }
                else
                {
                    var product = await _productRepository.GetProductByIdAsync(existingItem.ProductId);
                    if (product == null || product.Quantity + existingItem.Quantity < newItem.Quantity)
                    {
                        return BadRequest(new { errors = "Produto não existe ou não possui quantidade disponível" });
                    }

                    if (newItem.Quantity <= 0)
                        return BadRequest(new { errors = $"Não é possível adicionar uma quantidade igual ou menor que zero deste produto" });

                    product.Quantity += existingItem.Quantity - newItem.Quantity;
                    existingItem.Quantity = newItem.Quantity;
                    existingItem.Price = product.Price;
                    totalPriceProduct = newItem.Quantity * product.Price;
                    productsValues += totalPriceProduct;
                    KeyValue keyValue = new() { Key = product.Id, Value = product.Price };
                    keyValues.Add(keyValue);
                    _productRepository.UpdateNoCommit(product);
                }
            }

            _mapper.Map(updateModel, order);

            keyValues.ForEach(key => {
                order.Products.ForEach(prods =>{ 
                    if (prods.ProductId == key.Key)
                        prods.Price = key.Value;
                });
            });

            order.TotalValue = productsValues;
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);
            await _productRepository.SaveChangesAsync();

            var result = _mapper.Map<OrderReadModel>(order);
            return NoContent();
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }

    /// <summary>
    /// Finaliza o pedido
    /// </summary>
    /// <param name="id">Id do pedido</param>
    /// <param name="requestModel">Modelo de dados de entrada</param>
    /// <response code="204">Sucesso</response>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
    /// <response code="404">Chave não encontrada</response>
    [HttpPost("{id}/finish")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> FinishOrder([FromRoute] int id, [FromBody] FinalizeOrderRequestModel requestModel)
    {
        try
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound(new { errors = "Pedido não encontrado" });

            if (order.Products.Count <= 0)
                return BadRequest(new { errors = "Não é possível finalizar pedido sem produtos selecionados" });

            if (requestModel.PaymentOfType == PaymentOfType.Money && requestModel.PaymentValue < order.TotalValue)
                return BadRequest(new { errors = "Valor do pagamento em dinheiro não pode ser menor que o valor total do pedido, favor utilizar opção 'Fiado(em conta)'" });

            if (requestModel.PaymentOfType == PaymentOfType.Debitor && requestModel.PaymentValue > order.TotalValue)
                return BadRequest(new { errors = "Valor do pagamento não pode ser maior do que o valor do pedido, para o tipo de pagamento escolhido" });

            if (requestModel.PaymentOfType == PaymentOfType.ExtraMoney && requestModel.PaymentValue < order.TotalValue)
                return BadRequest(new { errors = "Valor do pagamento não pode ser menor do que o valor do pedido, para o tipo de pagamento escolhido" });

            if (order.Status != OrderStatus.InProgress)
                return BadRequest(new { errors = "Só é possível finalizar um pedido em andamento" });

            CustomerPerson? customerPerson = null;
            if (order.CustomerPersonId != null && order.CustomerPersonId > 0)
            {
                customerPerson = await _customerPersonRepository.GetCustomerPersonByIdAsync(order.CustomerPersonId.Value);
                if (customerPerson == null)
                    return NotFound(new { errors = "Cliente não encontrado" });
            }

            string valorTotalEmReal = order.TotalValue.ToString("C", new CultureInfo("pt-BR"));
            //Helper para verificar status e forma de pagamento, para fazer os cálculos devidos
            await _orderHelper.UpdateCalculatePaymentsOrder(order, requestModel, customerPerson);

            if (customerPerson != null)
            {
                try
                {
                    var message = $"*Cantina IBJ*\n\nOlá, *{customerPerson.Name}*.\n" +
                      $"Número do pedido: *{order.Id}*. Pedido finalizado!\n" +
                      $"_Segue dados do pedido:_ \n\n";

                    message += $"*Forma de pagamento:* {RandomHelpers.GetEnumDescription(order.PaymentOfType)}\n";

                    if (order.PaymentOfType == PaymentOfType.Money)
                    {
                        if (order.PaymentValue > order.TotalValue)
                        {
                            message += $"*Valor do pagamento:* R$ {order.PaymentValue:F2}\n";
                            message += $"*Valor do troco:* {order.ChangeValue:F2}\n";
                        }
                        else
                        {
                            message += $"*Valor do pagamento:* R$ {order.PaymentValue:F2}\n\n";
                        }
                    }

                    if (order.PaymentOfType == PaymentOfType.ExtraMoney)
                    {
                        message += $"*Valor do pagamento:* R$ {order.PaymentValue:F2}\n\n";
                    }

                    message += $"*Produtos:*\n";
                    // Concatena os produtos da lista
                    foreach (var orderProduct in order.Products)
                    {
                        var nomeProduto = orderProduct.Product.Name;
                        var precoUnitario = orderProduct.Price;
                        var quantidade = orderProduct.Quantity;
                        var precoTotalProduto = precoUnitario * quantidade;

                        message += $"- _{nomeProduto}_ - {quantidade} _Unid_ x R$ {precoUnitario:F2} = R$ {precoTotalProduto:F2}\n";
                    }
    
                    message += $"\n*Valor total do pedido:* R$ {order.TotalValue:F2}";

                    // Envia a mensagem usando o serviço WhatsApp
                    if (customerPerson.Phone != null)
                        await _whatsGWService.WhatsSendMessage("55" + customerPerson.Phone, message);
                }
                catch { }
            }

            try
            {
               _printerService.ImprimirPedido(order);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao imprimir: {ex.Message}");
            }

            return NoContent();
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }

    /// <summary>
    /// Cancela um pedido
    /// </summary>
    /// <param name="id"></param>
    /// <response code="204">Sucesso</response>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
    /// <response code="404">Chave não encontrada</response>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> CancelOrder([FromRoute] int id)
    {
        try
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            if (order is null)
                return NotFound(new { errors = "Pedido não encontrado" });

            if (order.Status != OrderStatus.InProgress)
                return BadRequest(new { errors = "Só é possível cancelar um pedido em andamento" });

            order.Status = OrderStatus.Canceled;
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);

            return NoContent();
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }

    /// <summary>
    /// Exclui um registro de um pedido
    /// </summary>
    /// <remarks>Deleta um pedido</remarks>
    /// <param name="id">Id do pedido</param>
    /// <response code="204">Sucesso</response>
    /// <response code="400">Modelo inválido</response>
    /// <response code="401">Não autorizado</response>
    /// <response code="403">Acesso negado</response>
    /// <response code="404">Chave não encontrada</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        try
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound(new { errors = "Pedido não encontrado" });

            //Vou deixar ser excluido um pedido finalizado, para caso o operador/usuario tenha errado o pedido
            //if (order.Status == OrderStatus.Finished)
            //    return BadRequest(new { errors = "Não é possível excluir um pedido finalizado" });

            //No caso de exclusao do pedido, reverte as alterações do produto relacionados a quantidade e disponibilidade
            if (order.Products != null)
            {
                foreach ( var orderProduct in order.Products)
                {
                    var product = await _productRepository.GetProductByIdAsync(orderProduct.ProductId);
                    if (product is null)
                        return NotFound(new { errors = "Produto não encontrado" });

                    product.Quantity += orderProduct.Quantity;
                    if (product.Quantity > 0) 
                    { 
                        product.Disponibility = true;
                    }

                    await _productRepository.UpdateAsync(product);
                }
            }

            if (order.Status == OrderStatus.Finished)
            {
                if (order.CustomerPersonId != null && order.CustomerPersonId > 0)
                {
                    var customerPerson = await _customerPersonRepository.GetCustomerPersonByIdAsync(order.CustomerPersonId.Value);

                    if (customerPerson != null)
                    {
                        // CASO 1: Era Fiado (Gerou dívida, agora vamos perdoar/limpar)
                        if (order.PaymentOfType == PaymentOfType.Debitor)
                        {
                            // Se ele devia 15, o balance estava -15. Somamos 15 para voltar a 0.
                            customerPerson.Balance += order.TotalValue;
                        }

                        // CASO 2: Era Crédito/ExtraMoney (Gerou saldo positivo, agora vamos retirar)
                        else if (order.PaymentOfType == PaymentOfType.ExtraMoney)
                        {
                            // Se ele deu 20 para uma conta de 15, ele ganhou 5 de saldo.
                            // Agora que excluímos, temos que tirar esses 5 que ele ganhou.
                            decimal saldoGanhoNoPedido = (order.PaymentValue ?? 0) - order.TotalValue;
                            customerPerson.Balance -= saldoGanhoNoPedido;
                        }

                        await _customerPersonRepository.UpdateAsync(customerPerson);
                    }
                }
            }

            order.IsDeleted = true;
            order.Status = OrderStatus.Excluded;
            order.UpdatedAt = DateTimeOffset.UtcNow;

            await _orderRepository.UpdateAsync(order);

            return NoContent();
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }

    /// <summary>
    /// Imprimir um pedido
    /// </summary>
    /// <remarks>Imprimir um pedido</remarks>
    /// <param name="id">Id do pedido</param>
    [HttpPost("{id}/orderPrinted")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> OrderPrinted([FromRoute] int id)
    {
        try
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound(new { errors = "Pedido não encontrado" });

            try
            {
                _printerService.ImprimirPedido(order);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao imprimir: {ex.Message}");
            }

            return NoContent();
        }
        catch (Exception e)
        {
            return LoggerBadRequest(e, _logger);
        }
    }
}