using CantinaIBJ.Model.Orders;

namespace CantinaIBJ.Model.Interfaces;

public interface IPrinterService
{
    void ImprimirPedido(Order pedido);
}