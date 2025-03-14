using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Gyvr.Mythril2D
{
    public class UIMenuManager : MonoBehaviour
    {
        [Header("UI Menus")]
        [SerializeField] private UIGameMenu m_gameMenu = null;
        [SerializeField] private UIShop m_shop = null;
        [SerializeField] private UICraft m_craft = null;
        [SerializeField] private UICharacter m_stats = null;
        [SerializeField] private UIWarehouse m_warehouse = null;
        [SerializeField] private UIInventory m_inventory = null;
        [SerializeField] private UIJournal m_journal = null;
        [SerializeField] private UIAbilities m_abilities = null;
        [SerializeField] private UISettings m_settings = null;
        [SerializeField] private UISave m_save = null;
        [SerializeField] private UIDeath m_death = null;

        public UIInventory inventory => m_inventory;
        public UIWarehouse warehouse => m_warehouse;
        public UIShop shop => m_shop;
        public UICraft craft => m_craft;

        private Stack<IUIMenu> m_menuStack = new Stack<IUIMenu>();

        private void Start()
        {
            GameManager.NotificationSystem.gameMenuRequested.AddListener(OnGameMenuRequested);
            GameManager.NotificationSystem.shopRequested.AddListener(OnShopRequested);
            GameManager.NotificationSystem.craftRequested.AddListener(OnCraftRequested);
            GameManager.NotificationSystem.statsRequested.AddListener(OnStatsRequested);
            GameManager.NotificationSystem.journalRequested.AddListener(OnJournalRequested);
            GameManager.NotificationSystem.warehouseRequested.AddListener(OnWarehouseRequested);
            GameManager.NotificationSystem.inventoryRequested.AddListener(OnInventoryRequested);
            GameManager.NotificationSystem.spellBookRequested.AddListener(OnAbilitiesRequested);
            GameManager.NotificationSystem.settingsRequested.AddListener(OnSettingsRequested);
            GameManager.NotificationSystem.saveMenuRequested.AddListener(OnSaveMenuRequested);
            GameManager.NotificationSystem.deathScreenRequested.AddListener(OnDeathScreenRequested);

            m_gameMenu.Init();
            m_shop.Init();
            m_craft.Init();
            m_stats.Init();
            m_journal.Init();
            m_warehouse.Init();
            m_inventory.Init();
            m_abilities.Init();
            m_settings.Init();
            m_save.Init();
            m_death.Init();

            GameManager.InputSystem.ui.cancel.performed += OnCancel;
        }

        private void OnDestroy()
        {
            GameManager.InputSystem.ui.cancel.performed -= OnCancel;
        }

        private void UpdateMenuNavigation(IUIMenu menu)
        {
            GameObject toSelect = menu.FindSomethingToSelect();

            if (!toSelect)
            {
                MonoBehaviour menuMonoBehaviour = (MonoBehaviour)m_menuStack.Peek();
                Selectable firstSelectable = menuMonoBehaviour.gameObject.GetComponentInChildren<Selectable>();

                if (firstSelectable)
                {
                    toSelect = firstSelectable.gameObject;
                }
            }

            GameManager.EventSystem.SetSelectedGameObject(toSelect);
        }

        public void ClearMenuStackOnDeath()
        {
            // 确保游戏状态正确调整，比如移除菜单层级
            while (m_menuStack.Count > 0)
            {
                IUIMenu menu = m_menuStack.Pop();
                menu.OnMenuPopped(); // 确保调用清理逻辑
                Hide(menu);
            }

            // 如果有其他依赖游戏状态的系统，也可以在这里做额外清理
            GameManager.GameStateSystem.RemoveLayer(EGameState.Menu);
            //GameManager.NotificationSystem.allMenusClosed.Invoke();
        }

        private void OnCancel(InputAction.CallbackContext context)
        {
            // Prevent exiting a menu if a dialogue is playing
            if (!GameManager.DialogueSystem.Main.IsPlaying())
            {
                PopMenu();
            }
        }

        private void PushMenu(IUIMenu menu, params object[] args)
        {
            //Debug.Log("PushMenu");

            if (m_menuStack.Count > 0 && m_menuStack.Peek() != null)
            {
                IUIMenu previousMenu = m_menuStack.Peek();
                previousMenu.EnableInteractions(false);
            }

            m_menuStack.Push(menu);
            menu.OnMenuPushed();
            GameManager.GameStateSystem.AddLayer(EGameState.Menu);
            Show(menu, args);
        }

        public void PopDeathMenu()
        {
            if (!GameManager.DialogueSystem.Main.IsPlaying())
            {
                IUIMenu menu = m_menuStack.Pop();
                Hide(menu);

                if (m_menuStack.Count > 0)
                {
                    Show(m_menuStack.Peek());
                }
            }
        }

        private void PopMenu()
        {
            if (m_menuStack.Count > 0 && m_menuStack.Peek().CanPop())
            {
                IUIMenu menu = m_menuStack.Pop();
                menu.OnMenuPopped();
                Hide(menu);

                if (m_menuStack.Count > 0)
                {
                    Show(m_menuStack.Peek());
                }
            }
        }

        private void Show(IUIMenu menu, params object[] args)
        {
            menu.Show(args);
            menu.EnableInteractions(true);

            GameManager.NotificationSystem.menuShowed.Invoke(menu);

            // Because some menus may use layouts, they may need at least one frame to be setup properly.
            // If we update menu navigation too early, the selection cursor feedback may move during the setup
            StartCoroutine(CoroutineHelpers.ExecuteInXFrames(1, () => UpdateMenuNavigation(menu)));
        }

        private void Hide(IUIMenu menu)
        {
            menu.Hide();
            GameManager.NotificationSystem.menuHid.Invoke(menu);
            GameManager.NotificationSystem.itemDetailsClosed.Invoke();
            GameManager.GameStateSystem.RemoveLayer(EGameState.Menu);
        }

        private void OnGameMenuRequested() => PushMenu(m_gameMenu);

        private void OnShopRequested(Shop shop) => PushMenu(m_shop, shop);

        private void OnCraftRequested(CraftingStation craftingStation) => PushMenu(m_craft, craftingStation);

        private void OnStatsRequested() => PushMenu(m_stats);

        private void OnJournalRequested() => PushMenu(m_journal);

        private void OnWarehouseRequested() => PushMenu(m_warehouse);

        private void OnInventoryRequested() => PushMenu(m_inventory);

        private void OnAbilitiesRequested() => PushMenu(m_abilities);

        private void OnSettingsRequested() => PushMenu(m_settings);

        private void OnSaveMenuRequested() => PushMenu(m_save);

        private void OnDeathScreenRequested() => PushMenu(m_death);
    }
}
