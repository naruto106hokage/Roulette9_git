using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class BalanceManager : MonoBehaviour
{
    public GameObject transactionDetailPrefab;
    public Transform transactionListParent;

    public TMP_Text balanceText;
    public TMP_Text[] balanceText2;
    public TMP_Text winningBalanceText;
    public TMP_InputField rechargeAmountInputField;
    public Button fetchBalanceButton;
    public Button rechargeButton;
    public Button fetchWinningBalanceButton;
    public Button[] amountButtons;

    private string balance;
    public bool rechargeScreen = true;

    public string authKey;
    private string baseURL;
    private void Awake()
    {
        Screen.orientation = ScreenOrientation.Portrait;
    }
    void Start()
    {
        if (rechargeScreen)
        {
            if (rechargeButton != null)
            {
                rechargeButton.onClick.AddListener(OnRechargeButtonClicked);
            }

            foreach (Button button in amountButtons)
            {
                Button localButton = button; // Create a local copy of the button variable
                localButton.onClick.AddListener(() => OnAmountButtonClicked(localButton));
            }
        }
    }

    private void OnRechargeButtonClicked()
    {
        string amountText = rechargeAmountInputField.text;
        if (!string.IsNullOrEmpty(amountText) && float.TryParse(amountText, out float amount))
        {
            StartCoroutine(RechargeBalanceRequest(amount));
        }
        else
        {
            Debug.LogWarning("Invalid recharge amount.");
        }
    }

    private void OnAmountButtonClicked(Button button)
    {
        if (button != null)
        {
            TMP_Text textComponent = button.GetComponentInChildren<TMP_Text>();
            if (textComponent != null && int.TryParse(textComponent.text, out int amount))
            {
                rechargeAmountInputField.text = amount.ToString();
            }
        }
    }

    public IEnumerator RechargeBalanceRequest(float amount)
    {
        string url = "https://indianpay.co.in/admin/paynow"; // The new endpoint for recharging balance
        Dictionary<string, string> headers = new Dictionary<string, string>
        {
            { "Content-Type", "application/json" }
        };

        // Generate a new payment order ID
        string orderId = GenerateOrderId();
        if (amount < 100)
        {
            amount = amount + 100;
        }

        // Define the new request parameters
        var requestBody = new RechargeRequestBody
        {
            merchantid = "INDIANPAY10053",
            orderid = orderId,
            amount = amount.ToString(),
            name = PlayerPrefs.GetString("playerName"),
            email = PlayerPrefs.GetString("playerEmail"),
            mobile = PlayerPrefs.GetString("PlayerNumber"),
            remark = "remark",
            type = "2",
            redirect_url = "https://ludo.ludosixer.com/"
        };

        string jsonString = JsonUtility.ToJson(requestBody);
        Debug.Log("jsonString: " + jsonString);

        // Convert JSON string to byte array
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);
        Debug.Log("bodyRaw: " + bodyRaw);

        // Create WWW request
        WWW request = new WWW(url, bodyRaw, new Dictionary<string, string>
        {
            { "Content-Type", "application/json" }
        });

        yield return request;

        // Check for errors
        if (!string.IsNullOrEmpty(request.error))
        {
            Debug.LogError("Error: " + request.error);
            SceneManager.LoadScene("OTP");
        }
        else
        {
            string jsonResponse = request.text;
            Debug.Log("Response: " + jsonResponse);

            // Parse the JSON response
            PaymentResponse response = JsonUtility.FromJson<PaymentResponse>(jsonResponse);

            if (response != null && response.status == "SUCCESS")
            {
                // Load the payment link in the default web browser
                Application.OpenURL(response.payment_link);
            }
            else
            {
                Debug.LogError("Payment failed or invalid response: " + (response != null ? response.status : "null response"));
            }
        }
    }

    // Method to generate a unique order ID
    private string GenerateOrderId()
    {
        return System.DateTime.Now.ToString("yyyyMMddHHmmss") + UnityEngine.Random.Range(1000, 9999).ToString();
    }


    [System.Serializable]
    public class PaymentResponse
    {
        public string status;
        public string amount;
        public string order_id;
        public string payment_link;
        public string gateway_txn;
    }

    [System.Serializable]
    public class RechargeRequestBody
    {
        public string merchantid;
        public string orderid;
        public string amount;
        public string name;
        public string email;
        public string mobile;
        public string remark;
        public string type;
        public string redirect_url;
    }

    [System.Serializable]
    public class ProfileResponse
    {
        public Meta meta;
        public ProfileData data;

        [System.Serializable]
        public class Meta
        {
            public string msg;
            public bool status;
        }

        [System.Serializable]
        public class ProfileData
        {
            public float winningBalance;
            public float topUpBalance;
        }
    }

    [System.Serializable]
    public class RechargeRequest
    {
        public float amount;
        public string transactionId;
    }

    [System.Serializable]
    public class RechargeListResponse
    {
        public Meta meta;
        public RechargeData[] data;

        [System.Serializable]
        public class Meta
        {
            public string msg;
            public bool status;
        }

        [System.Serializable]
        public class RechargeData
        {
            public string amount;
            public string transactionId;
            public long createdAt;
        }
    }

    [System.Serializable]
    public class RechargeResponse
    {
        public Meta meta;

        [System.Serializable]
        public class Meta
        {
            public string msg;
            public bool status;
        }
    }
}
