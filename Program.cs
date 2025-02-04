using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;

namespace TextRPG
{
    class Program
    {
        static void Main(string[] args)
        {
            // Init
            Player player = new Player();
            Shop shop = new Shop(player);
            Dungeon dungeon = new Dungeon(player);

            while (true)
            {
                Console.Clear();
                Console.WriteLine("스파르타 마을에 오신 여러분 환영합니다.");
                Console.WriteLine("이곳에서 던전으로 들어가기전 활동을 할 수 있습니다.\n");
                Console.WriteLine("1. 상태 보기");
                Console.WriteLine("2. 인벤토리");
                Console.WriteLine("3. 상점");
                Console.WriteLine("4. 던전입장");
                Console.WriteLine("5. 휴식하기");
                Console.WriteLine("0. 저장 및 나가기\n");

                int input = CheckInput(0, 5);
                
                switch (input)
                {
                    case 1: // 상태 보기
                        player.ShowStatus();
                        break;
                    case 2: // 인벤토리
                        player.ShowInventory();
                        break;
                    case 3: // 상점
                        shop.ShowMain();
                        break;
                    case 4: // 던전입장
                        dungeon.ShowMain();
                        break;
                    case 5: // 휴식
                        player.Rest();
                        break;
                }

                if (input == 0)
                {
                    player.SaveData();
                    shop.SaveData();
                    break;
                }
            }
        }

        public static int CheckInput(int min, int max)
        {
            Console.WriteLine("원하시는 행동을 입력해주세요.");

            while (true)
            {
                Console.Write(">> ");
                if (int.TryParse(Console.ReadLine(), out int input))
                {
                    if (input >= min && input <= max)
                        return input;
                }

                Console.WriteLine("잘못된 입력입니다.");
            }
        }
    }

    public class ItemData
    {
        private static ItemData instance;

        public static ItemData Instance
        {
            get 
            {
                if (instance == null)
                    instance = new ItemData();

                return instance;
            }
        }

        public List<Item> items;

        private ItemData()
        {
            items = new List<Item>();

            #region Init_Item
            ItemInfo info = new ItemInfo();

            info.type = Item.Type.ARMOR;
            info.name = "수련자 갑옷";
            info.ability = 5;
            info.gold = 1000;
            info.desc = "수련에 도움을 주는 갑옷입니다.";
            items.Add(new Item(info));

            info.type = Item.Type.ARMOR;
            info.name = "무쇠 갑옷";
            info.ability = 9;
            info.gold = 2000;
            info.desc = "무쇠로 만들어져 튼튼한 갑옷입니다.";
            items.Add(new Item(info));

            info.type = Item.Type.ARMOR;
            info.name = "스파르타의 갑옷";
            info.ability = 15;
            info.gold = 3500;
            info.desc = "스파르타 전사들이 사용했다는 전설의 갑옷입니다.";
            items.Add(new Item(info));

            info.type = Item.Type.ARMOR;
            info.name = "사기 갑옷";
            info.ability = 99;
            info.gold = 99999;
            info.desc = "스파르타 전사들이 사용했다는 전설의 갑옷입니다.";
            items.Add(new Item(info));

            info.type = Item.Type.WEAPON;
            info.name = "낡은 검";
            info.ability = 2;
            info.gold = 600;
            info.desc = "쉽게 볼 수 있는 낡은 검입니다.";
            items.Add(new Item(info));

            info.type = Item.Type.WEAPON;
            info.name = "청동 도끼";
            info.ability = 5;
            info.gold = 1500;
            info.desc = "어디선가 사용됐던거 같은 도끼입니다.";
            items.Add(new Item(info));

            info.type = Item.Type.WEAPON;
            info.name = "스파르타의 창";
            info.ability = 7;
            info.gold = 3500;
            info.desc = "스파르타의 전사들이 사용했다는 전설의 창입니다.";
            items.Add(new Item(info));

            info.type = Item.Type.WEAPON;
            info.name = "사기 검";
            info.ability = 99;
            info.gold = 99999;
            info.desc = "쉽게 볼 수 있는 낡은 검입니다.";
            items.Add(new Item(info));
            #endregion
        }
    }

    interface ICharacter
    {
        string name { get; }
        int level { get; set; }
        string job { get; set; }
        float attack { get; set; }
        float defense { get; set; }
        int health { get; set; }
        int maxHealth { get; set; }
        int gold { get; set; }
        int exp { get; set; }
    }

    public class Player : ICharacter
    {
        public string name { get; }
        public int level { get; set; }
        public string job { get; set; }
        public float attack { get; set; }
        public float defense { get; set; }
        public int health { get; set; }
        public int maxHealth { get; set; }
        public int gold { get; set; }
        public int exp { get; set; }
        
        public List<Item> items;
        public Item[] equipments;

        public Player() 
        {
            items = new List<Item>();
            equipments = new Item[(int)Item.Type.END];
            
            // SaveData 확인
            IniFile ini = new IniFile("Player.ini");

            if (ini.Open())
            {
                // satatus
                level = Convert.ToInt32(ini.GetValue("Level"));
                name = ini.GetValue("Name");
                job = ini.GetValue("Job");
                attack = Convert.ToSingle(ini.GetValue("Attack"));
                defense = Convert.ToSingle(ini.GetValue("Defense"));
                maxHealth = Convert.ToInt32(ini.GetValue("MaxHealth"));
                health = Convert.ToInt32(ini.GetValue("Health"));
                gold = Convert.ToInt32(ini.GetValue("Gold"));
                exp = Convert.ToInt32(ini.GetValue("Exp"));

                // inventory
                int idx = 0;
                while (true)
                {
                    string itemName = ini.GetValue($"Item{idx}");

                    if (string.IsNullOrEmpty(itemName))
                        break;
                    
                    foreach (Item item in ItemData.Instance.items)
                    {
                        if (item.name.Equals(itemName))
                        {
                            items.Add(item);
                            break;
                        }
                    }
                    idx++;
                }

                // equip
                string weapon = ini.GetValue("Weapon");
                string armor = ini.GetValue("Armor");

                foreach (Item item in items)
                {
                    if (!string.IsNullOrEmpty(weapon) && equipments[(int)Item.Type.WEAPON] == null)
                    {
                        if (item.name.Equals(weapon))
                            equipments[(int)Item.Type.WEAPON] = item;
                    }

                    if (!string.IsNullOrEmpty(armor) && equipments[(int)Item.Type.ARMOR] == null)
                    {
                        if (item.name.Equals(armor))
                            equipments[(int)Item.Type.ARMOR] = item;
                    }

                    if (equipments[(int)Item.Type.WEAPON] != null && equipments[(int)Item.Type.ARMOR] != null)
                        break;
                }
            }
            else
            {
                level = 1;
                name = "Test";
                job = "전사";
                attack = 10f;
                defense = 5f;
                maxHealth = 100;
                health = maxHealth;
                gold = 1000;
                exp = 0;
            }
        }

        public void SaveData()
        {
            IniFile ini = new IniFile("Player.ini");

            // status
            ini.SetValue("Level", level.ToString());
            ini.SetValue("Name", name);
            ini.SetValue("Job", job);
            ini.SetValue("Attack", attack.ToString());
            ini.SetValue("Defense", defense.ToString());
            ini.SetValue("MaxHealth", maxHealth.ToString());
            ini.SetValue("Health", health.ToString());
            ini.SetValue("Gold", gold.ToString());
            ini.SetValue("Exp", exp.ToString());

            // item
            for (int idx = 0; idx < items.Count; idx++)
            {
                ini.SetValue($"Item{idx}", items[idx].name);
            }

            // equip
            if (equipments[(int)Item.Type.WEAPON] != null)
                ini.SetValue("Weapon", equipments[(int)Item.Type.WEAPON].name);

            if (equipments[(int)Item.Type.ARMOR] != null)
                ini.SetValue("Armor", equipments[(int)Item.Type.ARMOR].name);

            ini.Write();
        }

        public void ShowStatus ()
        {
            Console.Clear();
            Console.WriteLine("상태 보기");
            Console.WriteLine("캐릭터의 정보가 표시됩니다.\n");
            Console.WriteLine($"Lv. {level}");
            Console.WriteLine($"{name} ({job})");
            Console.WriteLine($"공격력: {GetAttack()}");
            Console.WriteLine($"방어력: {GetDefense()}");
            Console.WriteLine($"체력: {health}");
            Console.WriteLine($"Gold: {gold} G\n");
            Console.WriteLine("0. 나가기\n");
            
            int input = Program.CheckInput(0, 0);

            switch (input)
            {
                case 0: // 나가기
                    break;
            }
        }

        public void Rest()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("휴식하기");
                Console.WriteLine($"500 G 를 내면 체력을 회복할 수 있습니다. (보유 골드 : {gold} G)\n");

                Console.WriteLine("1. 휴식하기");
                Console.WriteLine("0. 나가기\n");
            
                int input = Program.CheckInput(0, 1);

                if (input == 0)
                    break;

                if (gold >= 500)
                {
                    gold -= 500;
                    health += 100;

                    if (health > maxHealth)
                        health = maxHealth;

                    Console.WriteLine("휴식을 완료했습니다.");
                }
                else
                {
                    Console.WriteLine("Gold가 부족합니다.");
                }

                Thread.Sleep(500);
            }
        }

        public float GetAttack()
        {
            float atk = attack;
            if (equipments[(int)Item.Type.WEAPON] != null) { atk += equipments[(int)Item.Type.WEAPON].ability; }

            return atk;
        }

        public float GetDefense()
        {
            float Def = defense;
            if (equipments[(int)Item.Type.ARMOR] != null) { Def += equipments[(int)Item.Type.ARMOR].ability; }

            return Def;
        }

        public void GetExp()
        {
            int lastExp = exp;

            exp++;

            if (level <= exp)
            {
                level++;
                exp = 0;

                attack += 0.5f;
                defense += 1;

                Console.WriteLine("레벨 업!");
            }
            else
            {
                Console.WriteLine($"경험치 {lastExp} -> {exp}");
            }
        }

        public void ShowInventory()
        {
            int itemCnt = 0;

            Console.Clear();
            Console.WriteLine("인벤토리");
            Console.WriteLine("보유 중인 아이템을 관리할 수 있습니다.\n");

            Console.WriteLine("[아이템 목록]");
            foreach (Item item in items)
            {
                itemCnt++;

                string equipped = item.Equals(equipments[(int)item.type]) ? "[E]" : "";

                switch (item.type)
                {
                    case Item.Type.WEAPON:
                        Console.WriteLine($"- {itemCnt} {equipped}{item.name}\t|공격력 +{item.ability} |\t {item.desc}");
                        break;
                    case Item.Type.ARMOR:
                        Console.WriteLine($"- {itemCnt} {equipped}{item.name}\t|방어력 +{item.ability} |\t {item.desc}");
                        break;
                }
            }

            Console.WriteLine();
            Console.WriteLine("1. 장착 관리");
            Console.WriteLine("0. 나가기\n");

            int input = Program.CheckInput(0, 1);
            switch (input)
            {
                case 0: // 나가기
                    break;
                case 1: // 장착 관리
                    ShowEquipped();
                    break;
            }
        }

        void ShowEquipped()
        {
            while (true)
            {
                int itemCnt = 0;

                Console.Clear();
                Console.WriteLine("인벤토리 - 장착 관리");
                Console.WriteLine("보유 중인 아이템을 관리할 수 있습니다.\n");

                Console.WriteLine("[아이템 목록]");
                foreach (Item item in items)
                {
                    itemCnt++;

                    string equipped = item.Equals(equipments[(int)item.type]) ? "[E]" : "";

                    switch (item.type)
                    {
                        case Item.Type.WEAPON:
                            Console.WriteLine($"- {itemCnt} {equipped}{item.name}\t|공격력 +{item.ability} |\t {item.desc}");
                            break;
                        case Item.Type.ARMOR:
                            Console.WriteLine($"- {itemCnt} {equipped}{item.name}\t|방어력 +{item.ability} |\t {item.desc}");
                            break;
                    }
                }

                Console.WriteLine();
                Console.WriteLine("0. 나가기\n");

                int input = Program.CheckInput(0, items.Count);

                if (input == 0) // 나가기
                {
                    ShowInventory();
                    break;
                }
                else  // 장비 장착
                {
                    Item selectItem = items[input - 1];

                    if (equipments[(int)selectItem.type] != null && equipments[(int)selectItem.type].Equals(selectItem))
                        equipments[(int)selectItem.type] = null;
                    else
                        equipments[(int)selectItem.type] = selectItem;
                }
            }
        }

    }

    public class Shop
    {
        public List<Item> items;
        public Player player;

        private float ratio;

        public Shop(Player _player)
        {
            items = ItemData.Instance.items;
            player = _player;
            ratio = 0.85f;

            // SaveData 확인
            IniFile ini = new IniFile("Shop.ini");

            if (ini.Open())
            {
                for (int i = 0; i < items.Count; i++)
                    items[i].sold = Convert.ToBoolean(ini.GetValue($"Item{i}"));
            }
        }

        public void SaveData()
        {
            // SaveData 확인
            IniFile ini = new IniFile("Shop.ini");

            for (int i = 0; i < items.Count; i++)
            {
                ini.SetValue($"Item{i}", items[i].sold.ToString());
            }

            ini.Write();
        }

        public void ShowMain()
        {
            Console.Clear();
            Console.WriteLine("상점");
            Console.WriteLine("필요한 아이템을 얻을 수 있는 상점입니다.\n");

            Console.WriteLine("[보유 골드]");
            Console.WriteLine($"{player.gold} G\n");

            Console.WriteLine("[아이템 목록]");
            foreach (Item item in items)
            {
                string gold = item.sold ? "구매 완료" : item.gold.ToString() + " G";

                switch (item.type)
                {
                    case Item.Type.WEAPON:
                        Console.WriteLine($"- {item.name}\t|공격력 +{item.ability} | {item.desc}\t| {gold}");
                        break;
                    case Item.Type.ARMOR:
                        Console.WriteLine($"- {item.name}\t|방어력 +{item.ability} | {item.desc}\t| {gold}");
                        break;
                }
            }

            Console.WriteLine();
            Console.WriteLine("1. 아이템 구매");
            Console.WriteLine("2. 아이템 판매");
            Console.WriteLine("0. 나가기\n");

            int input = Program.CheckInput(0, 2);
            switch (input)
            {
                case 0: // 나가기
                    break;
                case 1: // 아이템 구매
                    ShowBuyItems();
                    break;
                case 2: // 아이템 판매
                    ShowSellItems();
                    break;
            }
        }

        void ShowBuyItems()
        {
            while (true)
            {
                int itemCnt = 0;

                Console.Clear();
                Console.WriteLine("상점 - 아이템 구매");
                Console.WriteLine("필요한 아이템을 얻을 수 있는 상점입니다.\n");

                Console.WriteLine("[보유 골드]");
                Console.WriteLine($"{player.gold} G\n");

                Console.WriteLine("[아이템 목록]");
                foreach (Item item in items)
                {
                    itemCnt++;

                    string gold = item.sold ? "구매 완료" : item.gold.ToString() + " G";

                    switch (item.type)
                    {
                        case Item.Type.WEAPON:
                            Console.WriteLine($"- {itemCnt} {item.name}\t|공격력 +{item.ability} | {item.desc}\t| {gold}");
                            break;
                        case Item.Type.ARMOR:
                            Console.WriteLine($"- {itemCnt} {item.name}\t|방어력 +{item.ability} | {item.desc}\t| {gold}");
                            break;
                    }
                }

                Console.WriteLine();
                Console.WriteLine("0. 나가기\n");
            
                int input = Program.CheckInput(0, items.Count);

                if (input == 0) // 나가기
                {
                    ShowMain();
                    break;
                }
                else
                {
                    Item selectItem = items[input - 1];

                    if (!selectItem.sold)
                    {
                        if (selectItem.gold <= player.gold)
                        {
                            // 아이템 구매 완료
                            selectItem.sold = true;
                            player.gold -= selectItem.gold;
                            player.items.Add(selectItem);

                            Console.WriteLine("구매를 완료했습니다.");
                        }
                        else
                        {
                            // 골드 부족
                            Console.WriteLine("Gold가 부족합니다.");
                        }

                    }
                    else
                        Console.WriteLine("이미 구매한 아이템입니다.");

                    Thread.Sleep(500); // 텍스트 출력 후 0.5초 딜레이
                }
            }
            
        }

        void ShowSellItems()
        {
            while (true)
            {
                int itemCnt = 0;

                Console.Clear();
                Console.WriteLine("상점 - 아이템 판매");
                Console.WriteLine("필요한 아이템을 얻을 수 있는 상점입니다.\n");

                Console.WriteLine("[보유 골드]");
                Console.WriteLine($"{player.gold} G\n");

                Console.WriteLine("[아이템 목록]");
                foreach (Item item in player.items)
                {
                    itemCnt++;

                    int gold = Convert.ToInt32(item.gold * ratio);
                    string equipped = item.Equals(player.equipments[(int)item.type]) ? "[E]" : "";

                    switch (item.type)
                    {
                        case Item.Type.WEAPON:
                            Console.WriteLine($"- {itemCnt} {equipped}{item.name}\t|공격력 +{item.ability} | {item.desc}\t| {gold} G");
                            break;
                        case Item.Type.ARMOR:
                            Console.WriteLine($"- {itemCnt} {equipped}{item.name}\t|방어력 +{item.ability} | {item.desc}\t| {gold} G");
                            break;
                    }
                }

                Console.WriteLine();
                Console.WriteLine("0. 나가기\n");

                int input = Program.CheckInput(0, player.items.Count);

                if (input == 0) // 나가기
                {
                    ShowMain();
                    break;
                }
                else
                {
                    Item selectItem = player.items[input - 1];
                    
                    // 아이템 판매 완료
                    selectItem.sold = false;
                    player.gold += Convert.ToInt32(selectItem.gold * ratio);

                    // 플레이어가 아이템을 장착하고 있다면 해제
                    if (player.equipments[(int)selectItem.type] == (selectItem))
                        player.equipments[(int)selectItem.type] = null;

                    player.items.Remove(selectItem);

                    Console.WriteLine("판매를 완료했습니다.");

                    Thread.Sleep(500); // 텍스트 출력 후 0.5초 딜레이
                }
            }
        }
    }

    public class Dungeon
    {
        public enum Difficulty { EASY = 1, NORMAL, HARD }

        Player player;
        Random rand;
        public Dungeon(Player _player)
        {
            player = _player;
            rand = new Random();
        }

        public void ShowMain()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("던전입장");
                Console.WriteLine("이곳에서 던전으로 들어가기전 활동을 할 수 있습니다.\n");

                Console.WriteLine("1. 쉬운 던전\t| 방어력 5이상 권장");
                Console.WriteLine("2. 일반 던전\t| 방어력 11이상 권장");
                Console.WriteLine("3. 어려운 던전\t| 방어력 17이상 권장");
                Console.WriteLine("0. 나가기\n");
            
                int input = Program.CheckInput(0, 3);

                if (input == 0)
                    break;
                else
                    DungeonEnter((Difficulty)input);
            }
        }

        void DungeonEnter(Difficulty eDiff)
        {
            float spec = 0;
            int reward = 0;
            string dungeonName = "";

            // 난이도별 권장 방어력 설정
            switch (eDiff)
            {
                case Difficulty.EASY:
                    spec = 5;
                    reward = 1000;
                    dungeonName = "쉬운 던전";
                    break;
                case Difficulty.NORMAL:
                    spec = 11;
                    reward = 1700;
                    dungeonName = "일반 던전";
                    break;
                case Difficulty.HARD:
                    spec = 17;
                    reward = 2500;
                    dungeonName = "어려운 던전";
                    break;
            }

            // 데미지
            int damage = rand.Next(20, 36) + (int)(spec - player.GetDefense());
            // 추가 보상
            //float rewardRatio = (rand.NextSingle() * player.GetAttack() + player.GetAttack()) / 100f;
            float rewardRatio = rand.Next((int)player.GetAttack(), (int)player.GetAttack() * 2) / 100f;
            int addReward = (int)(reward * rewardRatio);
            // 던전 클리어
            DungeonClear(damage, reward, addReward, dungeonName);
        }

        void DungeonClear(int damage, int reward, int addReward, string dungeonName)
        {
            int lastHealth = player.health;
            int lastGold = player.gold;

            // 플레이어 체력 감소
            if (damage < 0)
                damage = 0;
            player.health -= damage;

            Console.Clear();
            if (player.health > 0)
            {
                Console.WriteLine("던전 클리어");
                Console.WriteLine("축하합니다!!");
                Console.WriteLine($"{dungeonName}을 클리어 하였습니다.\n");

                Console.WriteLine("[탐험 결과]");
                Console.WriteLine($"체력 {lastHealth} -> {player.health}");

                player.gold += reward + (addReward);
                Console.WriteLine($"Gold {lastGold} -> {player.gold}");

                player.GetExp();
            }
            else
            {
                Console.WriteLine("던전 클리어 실패");

                player.health = 0;
                Console.WriteLine($"체력 {lastHealth} -> {player.health}");
            }

            Console.WriteLine();
            Console.WriteLine("0. 나가기\n");

            int input = Program.CheckInput(0, 0);
        }
    }

    interface IItem
    {
        bool sold { get; set; }
        int ability { get; }
        int gold { get; }
        string name { get; }
        string desc { get; }
    }

    public struct ItemInfo
    {
        public Item.Type type;
        public int ability;
        public int gold;
        public string name;
        public string desc;
    }

    public class Item : IItem
    {
        public enum Type { WEAPON, ARMOR, END }
        public bool sold { get; set; }
        public int ability { get; }
        public int gold { get; }
        public string name { get; }
        public string desc { get; }

        public Type type;

        public Item (ItemInfo info)
        {
            ability = info.ability;
            gold = info.gold;
            name = info.name;
            desc = info.desc;
            type = info.type;
            sold = false;
        }
    }

    public class IniFile
    {
        private string path;

        public Dictionary<string, string> datas;
        public IniFile(string _path)
        {
            path = _path;

            datas = new Dictionary<string, string>();
        }

        public void SetValue(string key, string value)
        {
            datas.Add(key, value);
        }

        public string GetValue(string key)
        {
            if (datas.ContainsKey(key))
                return datas[key];

            return string.Empty;
        }

        public void Write()
        {
            int idx = 0;
            string[] contens = new string[datas.Count];

            foreach (var data in datas)
            {
                contens[idx] = $"{data.Key}={data.Value}";
                idx++;
            }

            File.WriteAllLines(path, contens);
            Console.WriteLine($"{path}저장 성공");
        }

        public bool Open()
        {
            bool isOpen = File.Exists(path);

            if (isOpen)
            {
                foreach (string line in File.ReadAllLines(path))
                {
                    string[] split = line.Split('=', 2);
                    datas.Add(split[0], split[1]);
                }
            }

            return isOpen;
        }
    }
}