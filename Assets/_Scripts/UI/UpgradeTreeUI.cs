using UnityEngine;
using System.Collections.Generic;
using Godus.Core;

namespace Godus.UI
{
    /// <summary>
    /// Minimal upgrade tree. Press T to toggle. Buy nodes with currency.
    /// Shared tree — Prisoner's hidden branch is a data field (not implemented yet).
    /// </summary>
    public class UpgradeTreeUI : MonoBehaviour
    {
        [System.Serializable]
        public class UpgradeNode
        {
            public string id;
            public string label;
            public string description;
            public int cost;
            public bool purchased;
            public string classRestricted; // empty = all, "Prisoner" = hidden branch
            public System.Action onPurchase;
        }

        private List<UpgradeNode> _nodes;
        private bool _visible;
        private GUIStyle _btnStyle, _labelStyle, _titleStyle;
        private Vector2 _scrollPos;

        private void Awake()
        {
            _nodes = new List<UpgradeNode>
            {
                new() { id = "hp_up_1", label = "Vitality I",    description = "+20 Max HP",          cost = 50 },
                new() { id = "dmg_up_1", label = "Strength I",   description = "+5 Attack Damage",   cost = 75 },
                new() { id = "spd_up_1", label = "Agility I",    description = "+0.5 Move Speed",    cost = 60 },
                new() { id = "dash_cd",  label = "Quick Dash",   description = "-0.2s Dash CD",       cost = 100 },
                new() { id = "hidden_1", label = "???",          description = "Prisoner only",       cost = 200, classRestricted = "Prisoner" },
            };
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
                _visible = !_visible;
        }

        private void OnGUI()
        {
            if (!_visible) return;

            InitStyles();

            float w = 380, h = 400;
            float x = (Screen.width - w) / 2f, y = (Screen.height - h) / 2f;

            GUI.Box(new Rect(x, y, w, h), "");
            GUI.Label(new Rect(x + 15, y + 10, w - 30, 30), "Upgrade Tree", _titleStyle);
            GUI.Label(new Rect(x + 15, y + 40, w - 30, 20),
                $"Currency: {SaveManager.CurrentSave?.totalCurrency ?? 0}", _labelStyle);

            _scrollPos = GUI.BeginScrollView(
                new Rect(x + 10, y + 65, w - 20, h - 110),
                _scrollPos,
                new Rect(0, 0, w - 40, _nodes.Count * 65));

            float ny = 5;
            foreach (var node in _nodes)
            {
                bool isHidden = !string.IsNullOrEmpty(node.classRestricted);
                string label = isHidden && !node.purchased ? "???" : node.label;
                string desc = isHidden && !node.purchased ? "Locked — Prisoner only" : node.description;
                string costText = node.purchased ? "OWNED" : $"{node.cost}g";

                GUI.Box(new Rect(0, ny, w - 50, 58), "");
                GUI.Label(new Rect(10, ny + 5, 150, 20), label, _labelStyle);
                GUI.Label(new Rect(10, ny + 25, 250, 20), desc);

                if (!node.purchased && !isHidden && (SaveManager.CurrentSave?.totalCurrency ?? 0) >= node.cost)
                {
                    if (GUI.Button(new Rect(w - 140, ny + 15, 80, 25), costText, _btnStyle))
                    {
                        if (SaveManager.SpendCurrency(node.cost))
                        {
                            node.purchased = true;
                            SaveManager.PurchaseUpgrade(node.id);
                            node.onPurchase?.Invoke();
                        }
                    }
                }
                else
                {
                    GUI.Label(new Rect(w - 140, ny + 15, 80, 25), costText, _labelStyle);
                }

                ny += 62;
            }

            GUI.EndScrollView();

            if (GUI.Button(new Rect(x + w - 60, y + 6, 50, 20), "X", _btnStyle))
                _visible = false;
        }

        private void InitStyles()
        {
            if (_btnStyle != null) return;
            _btnStyle = new GUIStyle(GUI.skin.button) { fontSize = 14 };
            _labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 14, normal = { textColor = Color.white } };
            _titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold, normal = { textColor = new Color(1f, 0.85f, 0.1f) } };
        }
    }
}
