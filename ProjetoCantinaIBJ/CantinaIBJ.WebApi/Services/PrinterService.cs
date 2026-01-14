using CantinaIBJ.Framework.Helpers;
using CantinaIBJ.Model.Interfaces;
using CantinaIBJ.Model.Orders;
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;

public class PrinterService(IConfiguration configuration) : IPrinterService
{
    public void ImprimirPedido(Order pedido)
    {
        string nomeImpressora = "POS58 10.0.0.6";

        var e = new EPSON();

        var payload = ByteSplicer.Combine(
            e.CenterAlign(),
            e.PrintLine(" "),
            e.PrintLine(" "),
            e.PrintLine("--- CANTINA IBJ ---"),
            e.PrintLine($"Pedido #{pedido.Id}"),
            e.LeftAlign(),
            e.PrintLine($"Data: {DateTime.Now:g}"),
            e.PrintLine(" "),
            e.PrintLine("--------------------------------"),
            e.PrintLine(" ")
        );

        foreach (var item in pedido.Products)
        {
            var nomeProduto = item.Product.Name.RemoveAccents();
            payload = ByteSplicer.Combine(payload,
                e.PrintLine($"{item.Quantity}x {nomeProduto} - R$ {item.Product.Price:F2}")
            );
        }

        payload = ByteSplicer.Combine(payload,
            e.PrintLine(" "),
            e.PrintLine("--------------------------------"),
            e.RightAlign(),
            e.PrintLine(" "),
            e.PrintLine($"TOTAL: R$ {pedido.TotalValue:F2}"),
            e.PrintLine(" "),
            e.PrintLine(" "),
            e.FullCut()
        );

        RawPrinterHelper.SendBytesToPrinter(nomeImpressora, payload);
    }
}