using System.Linq;
using Haulory.Mobile.ViewModels;

namespace Haulory.Mobile.Views;

public partial class DeliverySignaturePage : ContentPage
{
    public DeliverySignaturePage(DeliverySignatureViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;

        vm.SignatureDrawable.InvalidateRequested = () => SignatureView.Invalidate();
    }

    private void OnStartInteraction(object sender, TouchEventArgs e)
    {
      

        if (BindingContext is not DeliverySignatureViewModel vm)
            return;

        var point = e.Touches.FirstOrDefault();
        if (point == default)
            return;

        vm.StartStroke(point);
    }

    private void OnDragInteraction(object sender, TouchEventArgs e)
    {
        if (BindingContext is not DeliverySignatureViewModel vm)
            return;

        var point = e.Touches.FirstOrDefault();
        if (point == default)
            return;

        vm.DragStroke(point);
    }

    private void OnEndInteraction(object sender, TouchEventArgs e)
    {
       

        if (BindingContext is DeliverySignatureViewModel vm)
            vm.EndStroke();
    }
}
