using Microsoft.Maui.Controls;

namespace Haulory.Mobile.ViewModels;

public class InductionRequirementDraft : BindableObject
{
    string _title = "";
    string _ppe = "";
    string _validDays = "";

    public string Title
    {
        get => _title;
        set { _title = value; OnPropertyChanged(); }
    }

    public string PpeRequired
    {
        get => _ppe;
        set { _ppe = value; OnPropertyChanged(); }
    }

    public string ValidForDaysText
    {
        get => _validDays;
        set { _validDays = value; OnPropertyChanged(); }
    }
    string _companyName = "";

    public string CompanyName
    {
        get => _companyName;
        set { _companyName = value; OnPropertyChanged(); }
    }
    public bool IsValid
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Title)) return false;

            if (!string.IsNullOrWhiteSpace(ValidForDaysText))
            {
                if (!int.TryParse(ValidForDaysText.Trim(), out var d)) return false;
                if (d <= 0) return false;
            }

            return true;
        }
    }
}