using TMPro;
using UnityEngine;

namespace _Project.Presentation.Menu
{
    public class JoinRoomFlowButton : NetworkFlowButton
    {
        private const int CodeLength = 5;

        [SerializeField] private TMP_InputField _codeInput;
        [SerializeField] private JoinByCodeView _codeView;

        protected override bool ValidateRequest()
        {
            if (IsCodeValid(_codeInput.text))
            {
                return true;
            }

            _codeView.ShowInvalidCode();
            return false;
        }

        protected override void OnFlowStart()
        {
            _codeView.ClearError();
        }

        protected override void RequestMatch()
        {
            Matchmaking.JoinRoom(_codeInput.text);
        }

        private bool IsCodeValid(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return false;
            }

            string trimmed = code.Trim();

            if (trimmed.Length != CodeLength)
            {
                return false;
            }

            foreach (char symbol in trimmed)
            {
                if (IsCodeSymbol(symbol) == false)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsCodeSymbol(char symbol)
        {
            bool isLatinLetter = (symbol >= 'A' && symbol <= 'Z') || (symbol >= 'a' && symbol <= 'z');
            bool isDigit = symbol >= '0' && symbol <= '9';
            return isLatinLetter || isDigit;
        }
    }
}
