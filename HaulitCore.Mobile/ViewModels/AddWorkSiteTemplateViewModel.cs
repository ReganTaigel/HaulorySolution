using HaulitCore.Application.Interfaces.Repositories;
using HaulitCore.Application.Interfaces.Services;
using HaulitCore.Domain.Entities;
using HaulitCore.Mobile.Views;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace HaulitCore.Mobile.ViewModels;

public class AddWorkSiteTemplateViewModel : BaseViewModel
{
    private readonly ISessionService _session;
    private readonly IWorkSiteRepository _sites;
    private readonly IInductionRequirementRepository _reqs;

    private bool _isSaving;

    // Site fields
    private string _name = "";
    private string _companyName = "";
    private string _addressLine1 = "";
    private string _suburb = "";
    private string _city = "";

    public string Name { get => _name; set { _name = value; OnPropertyChanged(); Refresh(); } }
    public string CompanyName { get => _companyName; set { _companyName = value; OnPropertyChanged(); Refresh(); } }
    public string AddressLine1 { get => _addressLine1; set { _addressLine1 = value; OnPropertyChanged(); } }
    public string Suburb { get => _suburb; set { _suburb = value; OnPropertyChanged(); } }
    public string City { get => _city; set { _city = value; OnPropertyChanged(); } }

    public ObservableCollection<InductionRequirementDraft> RequirementDrafts { get; } = new();

    public ICommand AddRequirementDraftCommand { get; }
    public ICommand RemoveRequirementDraftCommand { get; }
    public ICommand SaveAllCommand { get; }
    public ICommand CancelCommand { get; }

    public bool CanSave =>
        !_isSaving &&
        _session.IsAuthenticated &&
        !string.IsNullOrWhiteSpace(Name) &&
        RequirementDrafts.Count > 0 &&
        RequirementDrafts.All(r => r.IsValid);

    public AddWorkSiteTemplateViewModel(
        ISessionService session,
        IWorkSiteRepository sites,
        IInductionRequirementRepository reqs)
    {
        _session = session;
        _sites = sites;
        _reqs = reqs;

        // Start with one row so user doesn’t have to tap +Add first
        RequirementDrafts.Add(new InductionRequirementDraft());

        AddRequirementDraftCommand = new Command(() =>
        {
            RequirementDrafts.Add(new InductionRequirementDraft());
            Refresh();
        });

        RemoveRequirementDraftCommand = new Command<InductionRequirementDraft>(draft =>
        {
            if (draft == null) return;

            RequirementDrafts.Remove(draft);
            if (RequirementDrafts.Count == 0)
                RequirementDrafts.Add(new InductionRequirementDraft());

            Refresh();
        });

        SaveAllCommand = new Command(async () => await SaveAllAsync(), () => CanSave);
        CancelCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
    }

    public async Task EnsureSessionAsync()
    {
        if (!_session.IsAuthenticated)
            await _session.RestoreAsync();

        Refresh();
    }

    private async Task SaveAllAsync()
    {
        if (!CanSave) return;

        try
        {
            _isSaving = true;
            Refresh();

            if (!_session.IsAuthenticated)
                await _session.RestoreAsync();

            var ownerId = _session.CurrentOwnerId ?? Guid.Empty;
            if (ownerId == Guid.Empty)
            {
                await Shell.Current.DisplayAlertAsync("Not logged in", "Please log in again.", "OK");
                return;
            }

            // 1) Create site
            var site = new WorkSite(
                ownerUserId: ownerId,
                name: Name.Trim(),
                addressLine1: string.IsNullOrWhiteSpace(AddressLine1) ? null : AddressLine1.Trim(),
                addressLine2: null,
                suburb: string.IsNullOrWhiteSpace(Suburb) ? null : Suburb.Trim(),
                city: string.IsNullOrWhiteSpace(City) ? null : City.Trim(),
                region: null,
                postcode: null,
                country: null
            );

            await _sites.AddAsync(site);

            // 2) Create requirements
            foreach (var d in RequirementDrafts)
            {
                int? validDays = null;
                if (!string.IsNullOrWhiteSpace(d.ValidForDaysText))
                    validDays = int.Parse(d.ValidForDaysText.Trim());

                var req = new InductionRequirement(
                    ownerUserId: ownerId,
                    workSiteId: site.Id,
                    title: d.Title.Trim(),
                    validForDays: validDays,
                    ppeRequired: string.IsNullOrWhiteSpace(d.PpeRequired) ? null : d.PpeRequired.Trim(),
                    companyName: string.IsNullOrWhiteSpace(d.CompanyName) ? null : d.CompanyName.Trim()
                );

                await _reqs.AddAsync(req);
            }

            await Shell.Current.DisplayAlertAsync("Saved", "Work site and requirements created.", "OK");
            await Shell.Current.GoToAsync(nameof(InductionTemplatesPage));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Save failed", ex.Message, "OK");
        }
        finally
        {
            _isSaving = false;
            Refresh();
        }
    }

    private void Refresh()
    {
        OnPropertyChanged(nameof(CanSave));
        (SaveAllCommand as Command)?.ChangeCanExecute();
    }
}