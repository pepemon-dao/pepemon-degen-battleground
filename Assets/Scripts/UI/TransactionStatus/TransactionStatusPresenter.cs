using UnityEngine;
using TMPro;
using UniRx;

namespace Pepemon.UI.TransactionStatus
{
    public class TransactionStatusPresenter : MonoBehaviour
    {
        public TMP_Text txHashLabel;
        public TMP_Text descriptionLabel;
        public TMP_Text statusLabel;

        private void Start()
        {
            MessageBroker.Default.Receive<TransactionRequestEvent>()
                .Subscribe(ev =>
                {
                    txHashLabel.text = ev.Hash;
                    descriptionLabel.text = ev.Description;
                    statusLabel.text = "Waiting for Metamask ...";
                })
                .AddTo(this);

            MessageBroker.Default.Receive<TransactionSignedEvent>()
                .Subscribe(ev =>
                {
                    if (ev.Success)
                    {
                        statusLabel.text = "Waiting for confirmation ...";
                    }
                    else
                    {
                        statusLabel.text = "Cancelled in Metamask";
                    }
                })
                .AddTo(this);

            MessageBroker.Default.Receive<TransactionResultEvent>()
                .Subscribe(ev =>
                {
                    if (ev.Success)
                    {
                        statusLabel.text = "Confirmed";
                    }
                    else
                    {
                        statusLabel.text = "Reverted";
                    }
                })
                .AddTo(this);
        }
    }
}
