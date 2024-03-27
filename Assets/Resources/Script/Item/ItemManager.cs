using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public enum MONEYTYPE
{ 
    START = 0,
    SILVERCOIN = 0,
    SILVERCOINS = 100,
    GOLDCOIN = 1000,
    GOLDCOINS = 10000,
    END
}

public class ItemManager : MonoBehaviour
{
    private static ItemManager m_instance;
    public static ItemManager Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<ItemManager>();
                DontDestroyOnLoad(m_instance.gameObject);
            }
            return m_instance;
        }
    }
    private void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public readonly float ItemHoldingTime = 30.0f; //드랍된 아이템의 필드유지시간(해당시간 경과시 비활성화)
    public readonly int AmountOfItemPoolingList = 100; //드랍된 아이템의 필드유지시간(해당시간 경과시 비활성화)

    //생성할 아이템들의 Prefab
    [SerializeField] private Item_Money m_SilverCoinPrefab;
    [SerializeField] private Item_Money m_SilverCoinsPrefab;
    [SerializeField] private Item_Money m_GoldCoinPrefab;
    [SerializeField] private Item_Money m_GoldCoinsPrefab;
    [SerializeField] private ItemObjectV2 m_ItemObjectPrefab;

    private Dictionary<MONEYTYPE, Queue<Item_Money>> MoneyPoolingList = new Dictionary<MONEYTYPE, Queue<Item_Money>>();
    private List<Item_Money> ActivatedMoneyItem = new List<Item_Money>();
    private Queue<ItemObjectV2> ItemPoolingList = new Queue<ItemObjectV2>();
    private List<ItemObjectV2> ActivatedItemObject = new List<ItemObjectV2>();
    private Transform m_ItemPoolingLocation; // 풀링된 아이템들의 게임상 위치(폴더)
    private int CurActivatedSilverCoin;
    private int CurActivatedSilverCoins;
    private int CurActivatedGoldCoin;
    private int CurActivatedGoldCoins;
    private int CurActivatedItem;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        GameObject ItemPoolingLocation = new GameObject();
        ItemPoolingLocation.name = "ItemPoolingLocation";
        m_ItemPoolingLocation = ItemPoolingLocation.transform;

        CurActivatedSilverCoin = 0;
        CurActivatedSilverCoins = 0;
        CurActivatedGoldCoin = 0;
        CurActivatedGoldCoins = 0;
        CurActivatedItem = 0;
        MakePoolingList();
    }

    private void MakeMoney(Item_Money prefab)
    {
        if (prefab)
        {
            Item_Money NewMoneyObject = Instantiate(prefab, m_ItemPoolingLocation);
            if (!MoneyPoolingList.ContainsKey(prefab.m_eType))
            {
                MoneyPoolingList.Add(prefab.m_eType, new Queue<Item_Money>());
            }
            MoneyPoolingList[prefab.m_eType].Enqueue(NewMoneyObject);
            NewMoneyObject.gameObject.SetActive(false);
        }
    }
    private void MakeItemObject()
    {
        if (m_ItemObjectPrefab)
        {
            ItemObjectV2 NewItemObject = Instantiate(m_ItemObjectPrefab, m_ItemPoolingLocation);
            ItemPoolingList.Enqueue(NewItemObject);
            NewItemObject.gameObject.SetActive(false);
        }
    }
    public void CreateMoney(int Amount, Transform Tr)
    {
        if (Amount < (int)MONEYTYPE.SILVERCOINS)
            CreateSilverCoin(Amount, Tr);
        else if (Amount < (int)MONEYTYPE.GOLDCOIN)
            CreateSilverCoins(Amount, Tr);
        else if (Amount < (int)MONEYTYPE.GOLDCOINS)
            CreateGoldCoin(Amount, Tr);
        else 
            CreateGoldCoins(Amount, Tr);
        if (SoundManager.Instance)
            SoundManager.Instance.PlaySFX("DropItem");
    }
    private void CreateSilverCoin(int Amount, Transform Tr)
    {
        if (CurActivatedSilverCoin < AmountOfItemPoolingList)
        {
            if (MoneyPoolingList[MONEYTYPE.SILVERCOIN].Count <= 0) MakeMoney(m_SilverCoinPrefab);

            Item_Money NewMoneyItem = MoneyPoolingList[MONEYTYPE.SILVERCOIN].Dequeue();
            ActivatedMoneyItem.Add(NewMoneyItem);

            if (NewMoneyItem)
            {
                NewMoneyItem.Init(Amount, Tr);
                CurActivatedSilverCoin++;

                NewMoneyItem.MoneyObjectDisable += () => ActivatedMoneyItem.Remove(NewMoneyItem);
                NewMoneyItem.MoneyObjectDisable += () => MoneyPoolingList[NewMoneyItem.m_eType].Enqueue(NewMoneyItem);
                NewMoneyItem.MoneyObjectDisable += () => CurActivatedSilverCoin--;
            }
        }
    }
    private void CreateSilverCoins(int Amount, Transform Tr)
    {
        if (CurActivatedSilverCoins < AmountOfItemPoolingList)
        {
            if (MoneyPoolingList[MONEYTYPE.SILVERCOINS].Count <= 0) MakeMoney(m_SilverCoinsPrefab);

            Item_Money NewMoneyItem = MoneyPoolingList[MONEYTYPE.SILVERCOINS].Dequeue();
            ActivatedMoneyItem.Add(NewMoneyItem);

            if (NewMoneyItem)
            {
                NewMoneyItem.Init(Amount, Tr);
                CurActivatedSilverCoins++;

                NewMoneyItem.MoneyObjectDisable += () => ActivatedMoneyItem.Remove(NewMoneyItem);
                NewMoneyItem.MoneyObjectDisable += () => MoneyPoolingList[NewMoneyItem.m_eType].Enqueue(NewMoneyItem);
                NewMoneyItem.MoneyObjectDisable += () => CurActivatedSilverCoins--;
            }
        }
    }
    private void CreateGoldCoin(int Amount, Transform Tr)
    {
        if (CurActivatedGoldCoin < AmountOfItemPoolingList)
        {
            if (MoneyPoolingList[MONEYTYPE.GOLDCOIN].Count <= 0) MakeMoney(m_GoldCoinPrefab);

            Item_Money NewMoneyItem = MoneyPoolingList[MONEYTYPE.GOLDCOIN].Dequeue();
            ActivatedMoneyItem.Add(NewMoneyItem);

            if (NewMoneyItem)
            {
                NewMoneyItem.Init(Amount, Tr);
                CurActivatedGoldCoin++;

                NewMoneyItem.MoneyObjectDisable += () => ActivatedMoneyItem.Remove(NewMoneyItem);
                NewMoneyItem.MoneyObjectDisable += () => MoneyPoolingList[NewMoneyItem.m_eType].Enqueue(NewMoneyItem);
                NewMoneyItem.MoneyObjectDisable += () => CurActivatedGoldCoin--;
            }
        }
    }
    private void CreateGoldCoins(int Amount, Transform Tr)
    {
        if (CurActivatedGoldCoins < AmountOfItemPoolingList)
        {
            if (MoneyPoolingList[MONEYTYPE.GOLDCOINS].Count <= 0) MakeMoney(m_GoldCoinsPrefab);

            Item_Money NewMoneyItem = MoneyPoolingList[MONEYTYPE.GOLDCOINS].Dequeue();
            ActivatedMoneyItem.Add(NewMoneyItem);

            if (NewMoneyItem)
            {
                NewMoneyItem.Init(Amount, Tr);
                CurActivatedGoldCoins++;

                NewMoneyItem.MoneyObjectDisable += () => ActivatedMoneyItem.Remove(NewMoneyItem);
                NewMoneyItem.MoneyObjectDisable += () => MoneyPoolingList[NewMoneyItem.m_eType].Enqueue(NewMoneyItem);
                NewMoneyItem.MoneyObjectDisable += () => CurActivatedGoldCoins--;
            }
        }
    }
    public void CreateItemObject(int ItemUniqueNumber, int Quantity, Transform Tr)
    {
        if (CurActivatedItem < AmountOfItemPoolingList)
        {
            if (ItemPoolingList.Count <= 0) MakeItemObject();

            ItemObjectV2 NewItemObject = ItemPoolingList.Dequeue();
            ActivatedItemObject.Add(NewItemObject);

            if (NewItemObject)
            {
                NewItemObject.ItemDataInit(ItemUniqueNumber, Quantity, Tr);
                CurActivatedItem++;

                NewItemObject.DropItemDisalbe += () => ActivatedItemObject.Remove(NewItemObject);
                NewItemObject.DropItemDisalbe += () => ItemPoolingList.Enqueue(NewItemObject);
                NewItemObject.DropItemDisalbe += () => CurActivatedItem--;
            }
            if (SoundManager.Instance)
                SoundManager.Instance.PlaySFX("DropItem");
        }
    }
    private void MakePoolingList()
    {
        for(int i =0; i < AmountOfItemPoolingList; i++)
        {
            MakeMoney(m_SilverCoinPrefab);
            MakeMoney(m_SilverCoinsPrefab);
            MakeMoney(m_GoldCoinPrefab);
            MakeMoney(m_GoldCoinsPrefab);
            MakeItemObject();
        }
    }

    public void Clear()
    {
        ActivatedMoneyItem.Clear();
        MoneyPoolingList.Clear();
        ActivatedItemObject.Clear();
        ItemPoolingList.Clear();
    }
}
