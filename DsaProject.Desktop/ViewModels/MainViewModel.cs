using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DsaProject.Core;
using Microsoft.Win32;

namespace DsaProject.Desktop.ViewModels;

public class MainViewModel : ObservableObject
{
    public MainViewModel()
    {
        _dsaKey = new DsaKey();
        UpdateKeyParamsView();
        GenerateKeyCommand = new RelayCommand(GenerateKey);
        SignCommand = new RelayCommand(Sign);
        VerifyCommand = new RelayCommand(Verify);
        LoadPlainTextFileCommand = new RelayCommand(LoadPlainTextFile);
        LoadSignatureFromFileCommand = new RelayCommand(LoadSignatureFromFile);
        SaveSignatureToFileCommand = new RelayCommand(SaveSignatureToFile);
    }
    
    // TODO: zapisywanie i wczytywanie klucza, poprawic wyglad

    public ICommand GenerateKeyCommand { get; }
    public ICommand SignCommand { get; }
    public ICommand VerifyCommand { get; }
    public ICommand LoadPlainTextFileCommand { get; }
    public ICommand LoadSignatureFromFileCommand { get; }
    public ICommand SaveSignatureToFileCommand { get; }


    private DsaKey _dsaKey;

    #region Properites

    private string _q = null!;
    private string _p = null!;
    private string _g = null!;
    private string _x = null!;
    private string _y = null!;
    private string _plainText = null!;
    private string _signature;
    private bool _useFileAsInput;

    public string Q
    {
        get => _q;
        set
        {
            if (value == _q) return;
            _q = value;
            OnPropertyChanged();
        }
    }

    public string P
    {
        get => _p;
        set
        {
            if (value == _p) return;
            _p = value;
            OnPropertyChanged();
        }
    }

    public string G
    {
        get => _g;
        set
        {
            if (value == _g) return;
            _g = value;
            OnPropertyChanged();
        }
    }

    public string X
    {
        get => _x;
        set
        {
            if (value == _x) return;
            _x = value;
            OnPropertyChanged();
        }
    }

    public string Y
    {
        get => _y;
        set
        {
            if (value == _y) return;
            _y = value;
            OnPropertyChanged();
        }
    }

    public string PlainText
    {
        get => _plainText;
        set
        {
            if (value == _plainText) return;
            _plainText = value;
            OnPropertyChanged();
        }
    }

    public string Signature
    {
        get => _signature;
        set
        {
            if (value == _signature) return;
            _signature = value;
            OnPropertyChanged();
        }
    }

    public bool UseFileAsInput
    {
        get => _useFileAsInput;
        set
        {
            if (value == _useFileAsInput) return;
            _useFileAsInput = value;
            OnPropertyChanged();
        }
    }

    #endregion

    private string? _plainTextFileName;

    private void LoadPlainTextFile()
    {
        var fileDialog = new OpenFileDialog();
        if (fileDialog.ShowDialog() != true)
        {
            return;
        }

        _plainTextFileName = fileDialog.FileName;
        PlainText = "File Loaded";
        UseFileAsInput = true;
    }

    private void GenerateKey()
    {
        _dsaKey.GenerateKey();
        UpdateKeyParamsView();
    }

    private void UpdateKeyParamsView()
    {
        Q = Convert.ToBase64String(_dsaKey.Q.ToByteArray());
        P = Convert.ToBase64String(_dsaKey.P.ToByteArray());
        G = Convert.ToBase64String(_dsaKey.G.ToByteArray());
        X = Convert.ToBase64String(_dsaKey.X.ToByteArray());
        Y = Convert.ToBase64String(_dsaKey.Y.ToByteArray());
    }

    private void Sign()
    {
        BigInteger r, s;
        if (UseFileAsInput)
        {
            if (_plainTextFileName is not null)
            {
                using var file = File.OpenRead(_plainTextFileName);
                (r, s) = Dsa.Sign(file, _dsaKey);
            }
            else
            {
                return;
            }
        }
        else
        {
            var data = Encoding.UTF8.GetBytes(PlainText);
            (r, s) = Dsa.Sign(data, _dsaKey);
        }

        Signature = Convert.ToBase64String(r.ToByteArray()) + '\n' + Convert.ToBase64String(s.ToByteArray());
    }

    private void Verify()
    {
        BigInteger r, s;
        try
        {
            var signature = Signature
                .Split('\n')
                .Select(x => new BigInteger(Convert.FromBase64String(x)))
                .ToArray();

            r = signature[0];
            s = signature[1];
        }
        catch (Exception _)
        {
            MessageBox.Show("Fail");
            return;
        }

        bool result;

        if (UseFileAsInput)
        {
            if (_plainTextFileName is not null)
            {
                using var file = File.OpenRead(_plainTextFileName);
                result = Dsa.Verify(file, _dsaKey, r, s);
            }
            else
            {
                return;
            }
        }
        else
        {
            var data = Encoding.UTF8.GetBytes(PlainText);
            result = Dsa.Verify(data, _dsaKey, r, s);
        }

        MessageBox.Show(result ? "Success" : "Fail");
    }

    private void SaveSignatureToFile()
    {
        var fileDialog = new SaveFileDialog();
        if (fileDialog.ShowDialog() != true)
        {
            return;
        }

        File.WriteAllText(fileDialog.FileName, Signature);
    }
    

    private void LoadSignatureFromFile()
    {
        var fileDialog = new OpenFileDialog();
        if (fileDialog.ShowDialog() != true)
        {
            return;
        }

        Signature = File.ReadAllText(fileDialog.FileName);
    }
}