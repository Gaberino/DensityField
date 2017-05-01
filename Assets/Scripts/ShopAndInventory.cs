using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopAndInventory : MonoBehaviour {

	public List<ShopItem> items;

	// Use this for initialization
	void Start () {
		int basicPrice = MapGenerator.Instance.basePrice;

		ShopItem shipFuel = new ShopItem("Ship Fuel", 
			"Specialty Ship Fuel that can offer mass independent thrust to navigate out of the Density Field. *Buy this to win*",
			MapGenerator.Instance.minimumMineVariable / 2, 
			1);
		ShopItem miningMachine = (new ShopItem("Mining Machine",
			"A basic autonomous miner that can operate autonomously in the unique conditions of the density field. Basic models are slow and boast little storage, but can be upgraded",
			basicPrice,
			999));
		ShopItem remoteKiosk = (new ShopItem ("Remote Shop Kiosk",
			"These kiosks can be deployed on planetoids like mining machines, and offer access to shop services and complimentary oxygen. They do require chips as maintainence, which they will automatically draw from the planetoid they are deployed on.",
			basicPrice * 2,
			999));  
		ShopItem upgradeKit = (new ShopItem("Miner Upgrade Kit",
			"A standard upgrade kit which can be used on a mining machine to improve its functions permanantly. Can be done twice to each machine.",
			basicPrice * 3,
			999));
		ShopItem tetherUpgrade = (new ShopItem("Tether Capacity Upgrade",
			"An instantly applicable upgrade to the density tether capacity of your suit, allowing you to hold the grav data of more planetoids at once.",
			basicPrice * 4,
			7));
		ShopItem breathRecycler = (new ShopItem("Breath Recycler 3000",
			"An upgrade to your suit which allows it to filter your waste air back into the life support system under stress-free conditions",
			basicPrice * 5,
			1));
		ShopItem inventoryUpgrade = (new ShopItem("Pocket Expander",
			"No one quite understands how, but this device somehow increases a person's carrying capacity. Although, as a side effect, they can only retrieve things from the pocket after all the things put in there before have been used as well.",
			basicPrice * 5,
			2));
		items.Add(shipFuel);
		items.Add(miningMachine);
		items.Add(remoteKiosk);
		items.Add(upgradeKit);
		items.Add(tetherUpgrade);
		items.Add(breathRecycler);
		items.Add(inventoryUpgrade);
	}

	public void PurchaseItem(int chosenItemIndex){
		if (SpaceBroPlayer.Instance.DensityChipsHeld >= items[chosenItemIndex].itemPrice && items[chosenItemIndex].limitedStock > 0){
			SpaceBroPlayer.Instance.DensityChipsHeld -= items[chosenItemIndex].itemPrice;
			items[chosenItemIndex].limitedStock -= 1;

			if (items[chosenItemIndex].itemName.CompareTo("Ship Fuel") == 0){
				//win game
			}
			else if (items[chosenItemIndex].itemName.CompareTo("Mining Machine") == 0){
				//add mining machine to inventory stack
			}
			else if (items[chosenItemIndex].itemName.CompareTo("Remote Shop Kiosk") == 0){
				//add remote shop to  inventory stack
			}
			else if (items[chosenItemIndex].itemName.CompareTo("Miner Upgrade Kit") == 0){
				//add upgrade kit to inventory stack 
			}
			else if (items[chosenItemIndex].itemName.CompareTo("Tether Capacity Upgrade") == 0){
				//increase tether capacity by one and enable new HUD element
				SpaceBroPlayer.Instance.tetherCapacity += 1;
				HudManager.Instance.RefreshAvailableTethers();
			}
			else if (items[chosenItemIndex].itemName.CompareTo("Breath Recycler 3000") == 0){
				//set hasRecycler to true
				//SpaceBroPlayer.Instance.hasRecycler = true;
			}
			else if (items[chosenItemIndex].itemName.CompareTo("Pocket Expander") == 0){
				//increase max size of inventory stack by 1
				SpaceBroPlayer.Instance.inventorySize += 1;
			}
		}
	}
}

public class ShopItem {
	public string itemName;
	public string itemDescription;
	public int itemPrice;
	public int limitedStock;
	public bool purchased = false;

	public ShopItem (string inputName, string inputDesc, int inputPrice, int stock) {
		itemName = inputName;
		itemDescription = inputDesc;
		itemPrice = inputPrice;
		limitedStock = stock;
	}
}
