using System;
using UnityEngine;

namespace jshepler.ngu.mods.Popups
{
    internal class RatioCalcPopup : BasePopup
    {
        private const int COL1 = 100;
        private const int COL2 = 100;
        private const int COL3 = 150;
        private const int COL4 = 100;

        private static Menu _menu;
        private static string _resource;

        private static PCB _ratio => RatioCalc.Ratios[_menu];

        private static PCB _base = new() { power = 0, cap = 0, bars = 0 };
        private static PCB _shouldBe = new() { power = 0, cap = 0, bars = 0 };
        private static PCB _levelsNeeded = new() { power = 0, cap = 0, bars = 0 };
        private static PCB _expNeeded = new() { power = 0, cap = 0, bars = 0 };

        private static EnergyPurchases _energyPurchases;
        private static MagicPurchases _magicPurchases;
        private static Resource3Purchases _res3Purchases;

        internal RatioCalcPopup() : base(400f, 95f, 500f, 300f) { }

        //protected override void UpdateRect()
        //{
        //    base.UpdateRectCentered(500f, 300f);
        //}

        internal void Open(Menu menu)
        {
            var character = Plugin.Character;
            _energyPurchases = character.energyPurchases;
            _magicPurchases = character.magicPurchases;
            _res3Purchases = character.res3Purchases;

            _menu = menu;
            switch (menu)
            {
                case Menu.EXP_Energy:
                    _resource = "Energy";
                    _base = _baseEnergy;
                    _customPower = character.settings.customPowerAmount;
                    break;

                case Menu.EXP_Magic:
                    _resource = "Magic";
                    _base = _baseMagic;
                    _customPower = character.settings.customMagicPowerAmount;
                    break;

                case Menu.EXP_R3:
                    _resource = character.res3.res3Name;
                    _base = _baseRes3;
                    _customPower = character.settings.customRes3PowerAmount;
                    break;
            }

            updateCalculated();

            //var sf = UIScaler.CurrentScale();// Plugin.Character.tooltip.canvas.scaleFactor;
            //base.WindowRect.x = 400 * sf;// Screen.width / 2f - (base.WindowRect.width / 2f);
            //base.WindowRect.y = 95 * sf;// Screen.height / 2f - (base.WindowRect.height / 2f);

            base.Open();
        }

        private static GUIStyle _windowStyle;
        private static GUIStyle _rightAlignedLabel;
        private static GUIStyle _rightAlignedTextField;
        private static GUIStyle _rightAlignedTextFieldError;

        private void initStyles(Rect windowRect)
        {
            _windowStyle = new GUIStyle("box");
            _windowStyle.normal.background = Popup.CreateSolidColorTexture(windowRect, new Color32(30, 30, 30, 255));

            _rightAlignedLabel = new GUIStyle("label");
            _rightAlignedLabel.alignment = TextAnchor.LowerRight;

            _rightAlignedTextField = new GUIStyle("textField");
            _rightAlignedTextField.alignment = TextAnchor.LowerRight;

            _rightAlignedTextFieldError = new GUIStyle("textField");
            _rightAlignedTextFieldError.alignment = TextAnchor.LowerRight;
            _rightAlignedTextFieldError.normal.textColor = Color.red;
            _rightAlignedTextFieldError.focused.textColor = Color.red;
        }

        protected override void DrawWindow(Rect windowRect)
        {
            if (_windowStyle == null)
                initStyles(windowRect);

            GUILayout.BeginArea(windowRect, _windowStyle);
            drawTitle();

            GUILayout.BeginVertical("box");
            drawHeader();
            drawRatio();
            drawCalculated();
            drawBuyButtons();
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label(" ");
            GUILayout.EndHorizontal();
            drawCustomPurchases();

            GUILayout.EndArea();
        }

        private void drawTitle()
        {
            GUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.Label($"{_resource} Ratio Calculator");
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("×"))
                base.Close();

            GUILayout.EndHorizontal();
        }

        private static void drawHeader()
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label(" ", GUILayout.Width(COL1));
            GUILayout.Label("Power", _rightAlignedLabel, GUILayout.Width(COL2));
            GUILayout.Label("Cap (multiple of 250)", _rightAlignedLabel, GUILayout.Width(COL3));
            GUILayout.Label("Bars", _rightAlignedLabel, GUILayout.Width(COL4));

            GUILayout.EndHorizontal();
        }

        private static bool _capRatioNot250 = false;
        private static void drawRatio()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Ratio: ", _rightAlignedLabel, GUILayout.Width(COL1));

            if (long.TryParse(GUILayout.TextField(_ratio.power.ToString(), _rightAlignedTextField, GUILayout.Width(COL2)), out long rp))
            {
                if (rp > 0 && rp != _ratio.power)
                {
                    _ratio.power = rp;
                    updateCalculated();
                }
            }

            if (long.TryParse(GUILayout.TextField(_ratio.cap.ToString(), _capRatioNot250 ? _rightAlignedTextFieldError : _rightAlignedTextField, GUILayout.Width(COL3)), out long rc))
            {
                if (rc > 0 && rc != _ratio.cap)
                {
                    _capRatioNot250 = rc % 250 > 0;
                    _ratio.cap = rc;
                    updateCalculated();
                }
            }

            if (long.TryParse(GUILayout.TextField(_ratio.bars.ToString(), _rightAlignedTextField, GUILayout.Width(COL4)), out long rb))
            {
                if (rb > 0 && rb != _ratio.bars)
                {
                    _ratio.bars = rb;
                    updateCalculated();
                }
            }

            GUILayout.EndHorizontal();
        }

        private static void drawCalculated()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Base: ", _rightAlignedLabel, GUILayout.Width(COL1));
            GUILayout.Label($"{_base.power:#,##0} ", _rightAlignedLabel, GUILayout.Width(COL2));
            GUILayout.Label($"{_base.cap:#,##0} ", _rightAlignedLabel, GUILayout.Width(COL3));
            GUILayout.Label($"{_base.bars:#,##0} ", _rightAlignedLabel, GUILayout.Width(COL4));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Should Have: ", _rightAlignedLabel, GUILayout.Width(COL1));
            GUILayout.Label($"{_shouldBe.power:#,##0} ", _rightAlignedLabel, GUILayout.Width(COL2));
            GUILayout.Label($"{_shouldBe.cap:#,##0} ", _rightAlignedLabel, GUILayout.Width(COL3));
            GUILayout.Label($"{_shouldBe.bars:#,##0} ", _rightAlignedLabel, GUILayout.Width(COL4));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Levels Needed: ", _rightAlignedLabel, GUILayout.Width(COL1));
            GUILayout.Label($"{_levelsNeeded.power:#,##0} ", _rightAlignedLabel, GUILayout.Width(COL2));
            GUILayout.Label($"{_levelsNeeded.cap:#,##0} ", _rightAlignedLabel, GUILayout.Width(COL3));
            GUILayout.Label($"{_levelsNeeded.bars:#,##0} ", _rightAlignedLabel, GUILayout.Width(COL4));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Exp Needed: ", _rightAlignedLabel, GUILayout.Width(COL1));
            GUILayout.Label($"{_expNeeded.power:#,##0} ", _rightAlignedLabel, GUILayout.Width(COL2));
            GUILayout.Label($"{_expNeeded.cap:#,##0} ", _rightAlignedLabel, GUILayout.Width(COL3));
            GUILayout.Label($"{_expNeeded.bars:#,##0} ", _rightAlignedLabel, GUILayout.Width(COL4));
            GUILayout.EndHorizontal();
        }

        private static void drawBuyButtons()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(" ", GUILayout.Width(COL1));

            GUILayout.BeginHorizontal(GUILayout.Width(COL2));
            GUILayout.FlexibleSpace();
            if (_levelsNeeded.power > 0 && GUILayout.Button("buy"))
                buyPower();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(COL3));
            GUILayout.FlexibleSpace();
            if (_levelsNeeded.cap > 0 && GUILayout.Button("buy"))
                buyCap();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(COL4));
            GUILayout.FlexibleSpace();
            if (_levelsNeeded.bars > 0 && GUILayout.Button("buy"))
                buyBars();
            GUILayout.EndHorizontal();

            GUILayout.EndHorizontal();
        }

        private static long _customPower;
        private static void drawCustomPurchases()
        {
            GUILayout.BeginVertical("box");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Custom Amnts:", _rightAlignedLabel, GUILayout.Width(COL1));

            var root = _customPower / _ratio.power;
            if (long.TryParse(GUILayout.TextField($"{_customPower:0}", _rightAlignedTextField, GUILayout.Width(COL2)), out var cp))
            {
                if (cp != _customPower)
                {
                    root = cp / _ratio.power;
                    _customPower = cp;
                }
            }

            GUILayout.Label($"{root * _ratio.cap:0}", _rightAlignedLabel, GUILayout.Width(COL3));
            GUILayout.Label($"{root * _ratio.bars:0}", _rightAlignedLabel, GUILayout.Width(COL4));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("set custom purchases"))
                updateCustomInputs();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private static void updateCalculated()
        {
            var root = ((float)_base.power / _ratio.power).CeilToLong();
            _shouldBe.power = root * _ratio.power;
            _shouldBe.cap = root * _ratio.cap;
            _shouldBe.bars = root * _ratio.bars;

            if (_base.cap > _shouldBe.cap)
            {
                root = ((double)_base.cap / _ratio.cap).CeilToLong();
                _shouldBe.cap = root * _ratio.cap;
                _shouldBe.power = root * _ratio.power;
                _shouldBe.bars = root * _ratio.bars;
            }

            if (_base.bars > _shouldBe.bars)
            {
                root = ((float)_base.bars / _ratio.bars).CeilToLong();
                _shouldBe.bars = root * _ratio.bars;
                _shouldBe.power = root * _ratio.power;
                _shouldBe.cap = root * _ratio.cap;
            }

            // cap can only be in multiples of 250
            if (_shouldBe.cap % 250L != 0)
            {
                _shouldBe.cap = (_shouldBe.cap / 250f).CeilToLong() * 250L;
                root = _shouldBe.cap / _ratio.cap;
                _shouldBe.power = root * _ratio.power;
                _shouldBe.bars = root * _ratio.bars;
            }

            _levelsNeeded.power = _shouldBe.power - _base.power;
            _levelsNeeded.cap = _shouldBe.cap - _base.cap;
            _levelsNeeded.bars = _shouldBe.bars - _base.bars;

            _expNeeded.power = powerCost(_levelsNeeded.power);
            _expNeeded.cap = capCost(_levelsNeeded.cap);
            _expNeeded.bars = barsCost(_levelsNeeded.bars);
        }

        private static void updateCustomInputs()
        {
            var root = _customPower / _ratio.power;
            var cp = $"{_customPower:0}";
            var cc = $"{root * _ratio.cap:0}";
            var cb = $"{root * _ratio.bars:0}";

            switch (_menu)
            {
                case Menu.EXP_Energy:
                    _energyPurchases.powerInput.text = cp;
                    _energyPurchases.updateCustomPowerInput();
                    _energyPurchases.capInput.text = cc;
                    _energyPurchases.updateCustomCapInput();
                    _energyPurchases.barInput.text = cb;
                    _energyPurchases.updateCustomBarInput();
                    break;

                case Menu.EXP_Magic:
                    _magicPurchases.powerInput.text = cp;
                    _magicPurchases.updateCustomPowerInput();
                    _magicPurchases.capInput.text = cc;
                    _magicPurchases.updateCustomCapInput();
                    _magicPurchases.barInput.text = cb;
                    _magicPurchases.updateCustomBarInput();
                    break;

                case Menu.EXP_R3:
                    _res3Purchases.powerInput.text = cp;
                    _res3Purchases.updateCustomPowerInput();
                    _res3Purchases.capInput.text = cc;
                    _res3Purchases.updateCustomCapInput();
                    _res3Purchases.barInput.text = cb;
                    _res3Purchases.updateCustomBarInput();
                    break;
            }
        }

        private static void buyPower()
        {
            var exp = _exp;
            if (exp <= 0L)
                return;

            var maxCost = Math.Min(exp, _expNeeded.power);
            var costOf1 = powerCost(1L);
            var powerToBuy = maxCost / costOf1;
            var cost = powerCost(powerToBuy);
            if (cost <= 0L || powerToBuy <= 0L)
                return;

            switch (_menu)
            {
                case Menu.EXP_Energy:
                    _ep += powerToBuy;
                    _base = _baseEnergy;
                    _energyPurchases.refresh();
                    break;

                case Menu.EXP_Magic:
                    _mp += powerToBuy;
                    _base = _baseMagic;
                    _magicPurchases.refresh();
                    break;

                case Menu.EXP_R3:
                    _rp += powerToBuy;
                    _base = _baseRes3;
                    _res3Purchases.refresh();
                    break;
            }

            _exp -= cost;
            updateCalculated();
        }

        private static void buyCap()
        {
            var exp = _exp;
            if (exp <= 0L)
                return;

            var maxCost = Math.Min(exp, _expNeeded.cap);
            var costOf250 = capCost(250L);
            var capToBuy = maxCost / costOf250 * 250L;
            var cost = capCost(capToBuy);
            if (cost <= 0L || capToBuy <= 0L)
                return;

            switch (_menu)
            {
                case Menu.EXP_Energy:
                    _ec += capToBuy;
                    _base = _baseEnergy;
                    _energyPurchases.refresh();
                    break;

                case Menu.EXP_Magic:
                    _mc += capToBuy;
                    _base = _baseMagic;
                    _magicPurchases.refresh();
                    break;

                case Menu.EXP_R3:
                    _rc += capToBuy;
                    _base = _baseRes3;
                    _res3Purchases.refresh();
                    break;
            }

            _exp -= cost;
            updateCalculated();
        }

        private static void buyBars()
        {
            var exp = _exp;
            if (exp <= 0L)
                return;

            var maxCost = Math.Min(exp, _expNeeded.bars);
            var costOf1 = barsCost(1L);
            var barsToBuy = maxCost / costOf1;
            var cost = barsCost(barsToBuy);

            if (cost <= 0L || barsToBuy <= 0)
                return;

            switch (_menu)
            {
                case Menu.EXP_Energy:
                    _eb += barsToBuy;
                    _base = _baseEnergy;
                    _energyPurchases.refresh();
                    break;

                case Menu.EXP_Magic:
                    _mb += barsToBuy;
                    _base = _baseMagic;
                    _magicPurchases.refresh();
                    break;

                case Menu.EXP_R3:
                    _rb += barsToBuy;
                    _base = _baseRes3;
                    _res3Purchases.refresh();
                    break;
            }

            _exp -= cost;
            updateCalculated();
        }

        private static long powerCost(long power)
        {
            return _menu switch
            {
                Menu.EXP_Energy => epCost(power),
                Menu.EXP_Magic => mpCost(power),
                Menu.EXP_R3 => rpCost(power),
                _ => 0L
            };
        }

        private static long capCost(long cap)
        {
            return _menu switch
            {
                Menu.EXP_Energy => ecCost(cap),
                Menu.EXP_Magic => mcCost(cap),
                Menu.EXP_R3 => rcCost(cap),
                _ => 0L
            };
        }

        private static long barsCost(long bars)
        {
            return _menu switch
            {
                Menu.EXP_Energy => ebCost(bars),
                Menu.EXP_Magic => mbCost(bars),
                Menu.EXP_R3 => rbCost(bars),
                _ => 0L
            };
        }

        private static long epCost(long amount) => amount * 150L;
        private static long ecCost(long amount) => amount / 250L;
        private static long ebCost(long amount) => amount * 80L;
        private static long mpCost(long amount) => amount * 150L * 3L;
        private static long mcCost(long amount) => amount / 250L * 3L;
        private static long mbCost(long amount) => amount * 80L * 3;
        private static long rpCost(long amount) => amount * 150L * 100000L;
        private static long rcCost(long amount) => amount / 250L * 100000L;
        private static long rbCost(long amount) => amount * 80L * 100000L;

        private static float _ep
        {
            get => Plugin.Character.energyPower;
            set => Plugin.Character.energyPower = value;
        }

        private static long _ec
        {
            get => Plugin.Character.capEnergy;
            set => Plugin.Character.capEnergy = value;
        }

        private static long _eb
        {
            get => Plugin.Character.energyBars;
            set => Plugin.Character.energyBars = value;
        }

        private static float _mp
        {
            get => Plugin.Character.magic.magicPower;
            set => Plugin.Character.magic.magicPower = value;
        }

        private static long _mc
        {
            get => Plugin.Character.magic.capMagic;
            set => Plugin.Character.magic.capMagic = value;
        }

        private static long _mb
        {
            get => Plugin.Character.magic.magicPerBar;
            set => Plugin.Character.magic.magicPerBar = value;
        }

        private static float _rp
        {
            get => Plugin.Character.res3.res3Power;
            set => Plugin.Character.res3.res3Power = value;
        }

        private static long _rc
        {
            get => Plugin.Character.res3.capRes3;
            set => Plugin.Character.res3.capRes3 = value;
        }

        private static long _rb
        {
            get => Plugin.Character.res3.res3PerBar;
            set => Plugin.Character.res3.res3PerBar = value;
        }

        private static long _exp
        {
            get => Plugin.Character.realExp;
            set => Plugin.Character.realExp = value;
        }

        private static PCB _baseEnergy => new(_ep.RoundToLong(), _ec, _eb);
        private static PCB _baseMagic => new(_mp.RoundToLong(), _mc, _mb);
        private static PCB _baseRes3 => new(_rp.RoundToLong(), _rc, _rb);
    }
}
