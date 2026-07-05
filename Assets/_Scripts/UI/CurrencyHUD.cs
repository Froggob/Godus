using UnityEngine;
using Godus.Core;

namespace Godus.UI
{
    /// <summary>
    /// Minimal HUD — shows currency count. Subscribes to EventBus.
    /// Attach to a Canvas GameObject.
    /// </summary>
    public class CurrencyHUD : MonoBehaviour
    {
        private int _displayedCurrency;
        private GUIStyle _style;

        private void OnEnable()
        {
            EventBus.CurrencyChanged += OnCurrencyChanged;
            SaveManager.Load();
            _displayedCurrency = SaveManager.CurrentSave?.totalCurrency ?? 0;
        }

        private void OnDisable()
        {
            EventBus.CurrencyChanged -= OnCurrencyChanged;
        }

        private void OnCurrencyChanged(int newTotal)
        {
            _displayedCurrency = newTotal;
        }

        private void OnGUI()
        {
            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.label);
                _style.fontSize = 24;
                _style.normal.textColor = new Color(1f, 0.85f, 0.1f);
                _style.fontStyle = FontStyle.Bold;
            }

            GUI.Label(new Rect(20, 20, 200, 40), $"⚙ {_displayedCurrency}", _style);
        }
    }
}
