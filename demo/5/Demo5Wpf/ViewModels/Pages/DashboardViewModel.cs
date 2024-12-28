using Demo5;
using Maomi.I18n;
using Microsoft.Extensions.Localization;
using System.Collections.ObjectModel;
using Wpf.Ui;
using Wpf.Ui.Extensions;

namespace Demo5Wpf.ViewModels.Pages;

public partial class DashboardViewModel : ObservableObject
{
    private readonly ISnackbarService _snackbarService;
    private readonly IStringLocalizer _i18n;
    private readonly I18nResourceFactory _i18nResourceFactory;
    private readonly WpfI18nContext _i18nContext;

    public DashboardViewModel(ISnackbarService snackbarService, IStringLocalizer i18n, I18nResourceFactory i18nResourceFactory, WpfI18nContext i18nContext)
    {
        _snackbarService = snackbarService;
        _i18n = i18n;
        _i18nResourceFactory = i18nResourceFactory;
        _i18nContext = i18nContext;

        Languages = new ObservableCollection<string>(_i18nResourceFactory.SupportedCultures.Select(x => x.Name));
    }

    [ObservableProperty]
    private string _userName = "";

    [ObservableProperty]
    private string _email = "";

    [ObservableProperty]
    private string _phone = "";


    /// <summary>
    /// 当前选择语言.
    /// </summary>
    [ObservableProperty]
    private string _selectedLanguage = "zh-CN";

    /// <summary>
    /// 当前支持的语言列表.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _languages = new();

    [ObservableProperty]
    private int _counter = 0;

    [RelayCommand]
    private void OnCounterIncrement()
    {
        Counter++;
    }

    [RelayCommand]
    private void OnSetLanguage()
    {
        _i18nContext.SetLanguage(SelectedLanguage);
    }

    [RelayCommand]
    private void OnSave()
    {
        if (string.IsNullOrWhiteSpace(UserName))
        {
            _snackbarService.ShowDanger(_i18n["错误"], _i18n["用户名必填"]);
            return;
        }

        if (string.IsNullOrWhiteSpace(Email))
        {
            _snackbarService.ShowDanger(_i18n["错误"], _i18n["邮箱必填"]);
            return;
        }

        if (string.IsNullOrWhiteSpace(Phone))
        {
            _snackbarService.ShowDanger(_i18n["错误"], _i18n["手机号必填"]);
            return;
        }

        _snackbarService.ShowDanger(_i18n["成功"], _i18n["已保存信息"]);
    }
}
