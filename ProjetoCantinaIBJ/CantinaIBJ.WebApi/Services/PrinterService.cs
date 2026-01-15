using CantinaIBJ.Framework.Helpers;
using CantinaIBJ.Model.Interfaces;
using CantinaIBJ.Model.Orders;
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;

public class PrinterService(IConfiguration configuration) : IPrinterService
{
    private readonly string _nomeImpressora = configuration["Printer:Name"];
    private readonly string _qrCodePix = configuration["Printer:PixCopiaECola"];

    public void ImprimirPedido(Order pedido)
    {
        var e = new EPSON();

        var nameCustomerPerson = pedido.CustomerPerson != null ? pedido.CustomerPerson.Name : pedido.CustomerName;
        var payload = ByteSplicer.Combine(
            e.CenterAlign(),
            e.PrintLine(" "),
            e.PrintLine(" "),
            e.PrintLine("CANTINA IBJ"),
            e.PrintLine($"Pedido #{pedido.Id}"),
            e.LeftAlign(),
            e.PrintLine($"Data: {DateTime.Now:g}"),
            e.PrintLine($"Cliente: {nameCustomerPerson}"),
            e.PrintLine(" "),
            e.PrintLine("--------------------------------"),
            e.PrintLine(" "),
            e.PrintLine("QNTD  x  DESC.  x   VALOR UNIT."),
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
            e.LeftAlign(),
            e.PrintLine(" "),
            e.PrintLine($"Pagamento: {pedido.PaymentOfType.ToDescription().RemoveAccents()}"),
            e.RightAlign(),
            e.PrintLine(" "),
            e.PrintLine($"TOTAL: R$ {pedido.TotalValue:F2}")
        );

        if (pedido.PaymentOfType == CantinaIBJ.Model.Enumerations.PaymentOfType.PIX)
        {
            string chavePix = _qrCodePix;

            payload = ByteSplicer.Combine(payload,
                e.CenterAlign(),
                e.PrintLine(" "),
                e.PrintLine("Pague com Pix:"),
                e.PrintLine(" "),
                e.PrintQRCode(chavePix),
                e.PrintLine(" "),
                e.PrintLine("Obrigado pela preferencia!"),
                e.PrintLine(" "),
                e.FullCut()
            );
        }
        else if (pedido.PaymentOfType == CantinaIBJ.Model.Enumerations.PaymentOfType.Money)
        {
            payload = ByteSplicer.Combine(payload,
                e.RightAlign(),
                e.PrintLine($"Dinheiro: R$ {pedido.PaymentValue:F2}"),
                e.PrintLine($"Troco: R$ {pedido.ChangeValue:F2}"),
                e.PrintLine(" "),
                e.CenterAlign(),
                e.PrintLine("Obrigado pela preferencia!"),
                e.PrintLine(" "),
                e.FullCut()
            );
        }
        else
        {
            payload = ByteSplicer.Combine(payload,
                e.CenterAlign(),
                e.PrintLine(" "),
                e.PrintLine("Obrigado pela preferencia!"),
                e.PrintLine(" "),
                e.FullCut()
            );
        }

        RawPrinterHelper.SendBytesToPrinter(_nomeImpressora, payload);
    }
}