using CantinaIBJ.Model.Orders;

namespace CantinaIBJ.Model.Interfaces;

public interface IPrinterService
{
    void ImprimirPedido(Order pedido);

    /// <summary>Imprime um cupom de teste na impressora selecionada.</summary>
    void ImprimirTeste();
}