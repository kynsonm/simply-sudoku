using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

// Deriving the Purchaser class from IStoreListener enables it to receive messages from Unity Purchasing.
public class Purchaser : MonoBehaviour, IStoreListener
{
    private static IStoreController m_StoreController;          // The Unity Purchasing system.
    private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.

    [SerializeField]
    private List<string> UnlockableIDList; 

    IEnumerator Start()
    {
        // If we haven't set up the Unity Purchasing reference
        if (m_StoreController == null)
        {
            // Begin to configure our connection to Purchasing
            yield return new WaitForSeconds(1f);
            InitializePurchasing();
        } else {
            Debug.Log("Purchaser: Already an m_storeController present");
        }

        // Checks that all unlocked products are unlocked
        yield return new WaitForSeconds(1f);
        CheckStoreProducts();
    }

    // Unlocks every Product already purchased in StoreController
    //   Mostly just to double check purchases
    void CheckStoreProducts() {
        // Get objects that hold ID's of each product
        ShopMenu shop = GameObject.FindObjectOfType<ShopMenu>();
        Unlockable[] unlocks = GameObject.FindObjectsOfType<Unlockable>();
        foreach (Product prod in m_StoreController.products.all) {
            // If its not bought, continue
            if (!prod.hasReceipt) { continue; }
            string ID = prod.definition.id;
            // Check each unlockable
            foreach (var unlock in unlocks) {
                if (!unlock.isIAP || unlock.isUnlocked) { continue; }
                if (ID == unlock.ID) { unlock.Unlock(); }
            }
            // Check each IAP pack
            foreach (var pack in shop.getBuyablePacks()) {
                if (ID == pack.ID) { shop.Purchase(pack); }
            }
        }
    }

    public void InitializePurchasing() 
    {
        // If we have already connected to Purchasing ...
        if (IsInitialized())
        {
            // ... we are done here.
            Debug.Log("Purchaser: StoreController is already initialized");
            return;
        }

        // Create a builder, first passing in a suite of Unity provided stores.
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        /*
        // Add a product to sell / restore by way of its identifier, associating the general identifier
        // with its store-specific identifiers.
        builder.AddProduct(kProductIDConsumable, ProductType.Consumable);
        // Continue adding the non-consumable product.
        builder.AddProduct(kProductIDNonConsumable, ProductType.NonConsumable);
        */
        string log = "Purchaser: ADDING PRODUCTS:\n";
        foreach (string ID in UnlockableIDList) {
            if (ID.Contains("coin")) {
                builder.AddProduct(ID, ProductType.Consumable);
            } else {
                builder.AddProduct(ID, ProductType.NonConsumable);
            }
            log += "-- Adding product ID: " + ID + "\n";
        }
        foreach (Unlockable unl in GameObject.FindObjectsOfType<Unlockable>(true)) {
            if (UnlockableIDList.Contains(unl.ID)) { continue; }
            if (!unl.isIAP) { continue; }
            builder.AddProduct(unl.ID, ProductType.NonConsumable);
            log += "-- Adding product ID from Unlockable: " + unl.ID + "\n";
        }
        Debug.Log(log);

        // Kick off the remainder of the set-up with an asynchrounous call, passing the configuration 
        // and this class' instance. Expect a response either in OnInitialized or OnInitializeFailed.
        UnityPurchasing.Initialize(this, builder);
    }


    private bool IsInitialized()
    {
        // Only say we are initialized if both the Purchasing references are set.
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }


    public void BuyProductID(string productId)
    {
        // If Purchasing has been initialized ...
        if (IsInitialized())
        {
            // ... look up the Product reference with the general product identifier and the Purchasing 
            // system's products collection.
            Product product = m_StoreController.products.WithID(productId);

            // If the look up found a product for this device's store and that product is ready to be sold ... 
            if (product != null && product.availableToPurchase)
            {
                Debug.Log(string.Format("Purchaser: Purchasing product asychronously: '{0}'", product.definition.id));
                // ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
                // asynchronously.
                m_StoreController.InitiatePurchase(product);
            }
            // Otherwise ...
            else if (product == null)
            {
                // ... report the product look-up failure situation  
                Debug.Log($"Purchaser: BuyProductID: FAIL. Not purchasing product, product w/ ID {productId} is not found");
            }
            else if (product.hasReceipt) {
                Debug.Log($"Purchaser: BuyProductID: FAIL. Product has already been purchased");
                // This one's on me:
                // Unlocks all unlocks that have been purchased
                CheckStoreProducts();
            }
            else {
                Debug.Log($"Purchaser: BuyProductID: FAIL. Not purchasing product, product is not available for purchase");
            }
        }
        // Otherwise ...
        else
        {
            // ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or 
            // retrying initiailization.
            Debug.Log("Purchaser: BuyProductID FAIL. Not initialized.");
        }
    }


    // Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google. 
    // Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
    public void RestorePurchases()
    {
        // If Purchasing has not yet been set up ...
        if (!IsInitialized())
        {
            // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
            Debug.Log("Purchaser: RestorePurchases FAIL. Not initialized.");
            return;
        }

        // If we are running on an Apple device ... 
        if (Application.platform == RuntimePlatform.IPhonePlayer || 
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            // ... begin restoring purchases
            Debug.Log("Purchaser: RestorePurchases started ...");

            // Fetch the Apple store-specific subsystem.
            var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
            // Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
            // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
            apple.RestoreTransactions((result) => {
                // The first phase of restoration. If no more responses are received on ProcessPurchase then 
                // no purchases are available to be restored.
                Debug.Log("Purchaser: RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
            
                // This one's on me:
                // Unlocks all unlocks that have been purchased
                CheckStoreProducts();
            });
        }
        // Otherwise ...
        else
        {
            // We are not running on an Apple device. No work is necessary to restore purchases.
            Debug.Log("Purchaser: RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
        }
    }


    //  
    // --- IStoreListener
    //

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        // Purchasing has succeeded initializing. Collect our Purchasing references.
        Debug.Log("Purchaser: OnInitialized: PASS");

        // Overall Purchasing system, configured with products for this application.
        m_StoreController = controller;
        // Store specific subsystem, for accessing device-specific store features.
        m_StoreExtensionProvider = extensions;
    }


    public void OnInitializeFailed(InitializationFailureReason error)
    {
        // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
        Debug.Log("Purchaser: OnInitializeFailed InitializationFailureReason:" + error);
    }


    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args) 
    {
        // Check if ID exists in Unlocks
        string ID = args.purchasedProduct.definition.id;
        if (!Unlocks.IDExists(ID)) {
            Debug.Log($"Purchaser: ProcessPurchace: FAIL. Unrecognized product ID: '{ID}'");
            return PurchaseProcessingResult.Complete;
        }

        // Check if ID is already purchased
        bool isUnlocked = Unlocks.isUnlocked(ID);
        if (isUnlocked) {
            Debug.Log($"Purchaser: ProcessPurchase: INV. Already purchased product ID: '{ID}'");
            return PurchaseProcessingResult.Complete;
        }

        // Get the unlock manager and shop menu
        ShopMenu shopMenu = GameObject.FindObjectOfType<ShopMenu>();
        if (shopMenu == null) {
            Debug.LogWarning("Purchaser: ProcessPurchase: COULD NOT FIND SHOP MENU -- CANCELLING PURCHASE");
            return PurchaseProcessingResult.Complete;
        }
        UnlockManager unlockManager = GameObject.FindObjectOfType<UnlockManager>();
        if (unlockManager == null) {
            Debug.LogWarning("Purchaser: ProcessingPurchase: COULD NOT FIND UNLOCK MANAGER -- CANCELLING PURCHASE");
            return PurchaseProcessingResult.Complete;
        }
        if (unlockManager.menuIsOpen) { unlockManager.EndPurchase(); }

        // Check in Unlockable's
        foreach (Unlockable unlock in GameObject.FindObjectsOfType<Unlockable>(true)) {
            if (unlock.ID == ID && unlock.isIAP) {
                unlock.Unlock();
                shopMenu.Purchase(unlock);
                Debug.Log($"Purchaser: ProcessPurchase: PASS. Unlockable Product: '{ID}'");
                PlayerInfo.PurchaseHasBeenMade = true;
                return PurchaseProcessingResult.Complete;
            }
        }

        // Check in coin packs and IAP packs
        CoinPack coinPack = shopMenu.FindCoinPack(ID);
        if (coinPack != null) {
            shopMenu.Purchase(coinPack);
            Debug.Log($"Purchaser: ProcessingPurchase: PASS. Coin Pack Product: '{ID}'");
            PlayerInfo.PurchaseHasBeenMade = true;
            return PurchaseProcessingResult.Complete;
        }
        IAPPack iapPack = shopMenu.FindIAPPack(ID);
        if (iapPack != null) {
            shopMenu.Purchase(iapPack);
            Debug.Log($"Purchaser: ProcessingPurchase: PASS. IAP Pack Product: '{ID}'");
            PlayerInfo.PurchaseHasBeenMade = true;
            return PurchaseProcessingResult.Complete;
        }

        Debug.Log($"Purchaser: ProcessPurchase: FAIL. Unrecognized product: '{ID}'");

        // Return a flag indicating whether this product has completely been received, or if the application needs 
        // to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
        // saving purchased products to the cloud, and when that save is delayed. 
        return PurchaseProcessingResult.Complete;
    }


    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
        // this reason with the user to guide their troubleshooting actions.
        Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
    }
}
