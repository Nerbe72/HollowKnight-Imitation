using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Pause : MonoBehaviour
{
    public static Pause instance;

    [SerializeField] private Sprite[] frameTopSprites;
    [SerializeField] private Sprite[] frameBottomSprites;

    private int spriteTopCount;
    private int spriteTopCheck;

    private int spriteBottomCount;
    private int spriteBottomCheck;

    private UIDocument m_uiDocument;
    public PlayerInput m_input;

    private VisualElement root;
    private Button m_resumeGame;
    private Button m_exitGame;
    private VisualElement m_frameTop;
    private VisualElement m_frameBottom;

    private IVisualElementScheduledItem m_taskTop;
    private IVisualElementScheduledItem m_taskBottom;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else
        {
            Destroy(gameObject);
            return;
        }
        Init();
    }

    private void Init()
    {
        m_input = GetComponent<PlayerInput>();

        m_uiDocument = GetComponent<UIDocument>();
        root = m_uiDocument.rootVisualElement;

        m_resumeGame = root.Q<Button>("resumeGame");
        m_exitGame = root.Q<Button>("exitGame");
        m_frameTop = root.Q<VisualElement>("frameTop");
        m_frameBottom = root.Q<VisualElement>("frameBottom");

        m_resumeGame.clicked += ResumeGame;
        m_exitGame.clicked += Exit;

        root.style.display = DisplayStyle.None;

        spriteTopCount = frameTopSprites.Length;
        spriteBottomCount = frameBottomSprites.Length;

    }

    private void PauseGame()
    {
        root.style.display = DisplayStyle.Flex;
        m_taskTop = m_frameTop.schedule.Execute(SwapTopSprite).Every(50);
        m_taskBottom = m_frameBottom.schedule.Execute(SwapBottomSprite).Every(50);
        PlayerController.instance.m_playerInput.DeactivateInput();
        InventoryUI.instance.ActiveInputToggle(false);
        Time.timeScale = 0f;
    }

    private void SwapTopSprite()
    {
        if (spriteTopCheck >= spriteTopCount)
        {
            spriteTopCheck = 0;
            m_taskTop.Pause();
            return;
        }

        m_frameTop.style.backgroundImage = new StyleBackground(frameTopSprites[spriteTopCheck++]);
    }

    private void SwapBottomSprite()
    {
        if (spriteBottomCheck >= spriteBottomCount)
        {
            spriteBottomCheck = 0;
            m_taskBottom.Pause();
            return;
        }

        m_frameBottom.style.backgroundImage = new StyleBackground(frameBottomSprites[spriteBottomCheck++]);
    }

    private void ResumeGame()
    {
        root.style.display = DisplayStyle.None;
        Time.timeScale = 1f;
        PlayerController.instance.m_playerInput.ActivateInput();
        InventoryUI.instance.ActiveInputToggle(true);
    }

    private void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnPause(InputValue _value)
    {
        float input = _value.Get<float>();
        if (input == 1f)
        {
            if (root.style.display == DisplayStyle.None)
                PauseGame();
            else
                ResumeGame();
        }
    }
}
