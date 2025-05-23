using System.Collections;
using System.Collections.Generic;
using Unigine;

[Component(PropertyGuid = "5456cc7115d472b899297af3d1d25a9ed94d7c7d")]
public class Information : Component
{
    // Путь к JSON-файлу с информацией, задается в редакторе
    [ShowInEditor]
    [ParameterFile(Filter = ".json")]
    private string jsonFile = "null";

    // Ссылка на компонент главного меню, задается в редакторе
    [ShowInEditor]
    private UIMainMenu mainMenu = null;

    // Основной контейнер для интерфейса
    private WidgetVBox VBox;

    // Фоновое изображение
    private WidgetSprite background;

    // Прозрачное PNG на позиции contentLabel
    private WidgetSprite textImage;

    // Метки для отображения заголовка, контента и номера блока
    private WidgetLabel titleLabel, contentLabel, numberLabel;

    // Изображение для текущего блока
    private WidgetSprite blockImage;

    // Индекс текущего блока
    private int currentBlockIndex;

    // Объекты для работы с JSON
    private Json json, jsonBlocks;

    // Структура для хранения данных блока
    struct Block
    {
        public string title; // Заголовок блока
        public string content; // Контент блока (строка)
        public string image; // Путь к фоновому изображению (или null)
        public string image2; // Путь к PNG на позиции contentLabel (или null)
    }

    // Список блоков
    private List<Block> blocks = new List<Block>();

    // Инициализация компонента
    public void Init()
    {
        // Проверяем, что WindowManager.MainWindow доступен
        if (WindowManager.MainWindow == null)
        {
            Log.Error("MainWindow is not initialized.\n");
            return;
        }

        json = new Json(); // Создаем объект для работы с JSON
        CreateUI(); // Создаем основной интерфейс
        LoadBlocks(); // Загружаем блоки из JSON
    }

    // Обновление каждый кадр
    public void Update()
    {
        // Проверяем нажатие пробела для перелистывания
        if (Input.IsKeyDown(Input.KEY.SPACE) && VBox != null && !VBox.Hidden)
        {
            NextBlock(); // Вызываем логику перелистывания
        }
    }

    // Проверка, активен ли интерфейс (true, если скрыт)
    public bool GetActiveInfo()
    {
        return VBox.Hidden;
    }

    // Запуск отображения информации
    public void StartInformation()
    {
        if (blocks.Count == 0)
        {
            Log.Error("No blocks loaded. Check JSON file.\n");
            return;
        }

        if (VBox == null)
        {
            Log.Error("VBox is not initialized.\n");
            return;
        }

        VBox.Hidden = false; // Показываем интерфейс
        currentBlockIndex = 0; // Начинаем с первого блока
        ShowBlock(); // Показываем первый блок
    }

    // Отображение текущего блока
    private void ShowBlock()
    {
        if (currentBlockIndex < 0 || currentBlockIndex >= blocks.Count)
        {
            Log.Error("Invalid block index: {0}\n", currentBlockIndex);
            return;
        }

        if (titleLabel == null || contentLabel == null || numberLabel == null || textImage == null)
        {
            Log.Error("UI labels or textImage are not initialized.\n");
            return;
        }

        Block block = blocks[currentBlockIndex];
        titleLabel.Text = block.title ?? "No Title"; // Устанавливаем заголовок
        contentLabel.Text = block.content ?? "No Content"; // Устанавливаем контент
        numberLabel.Text = (currentBlockIndex + 1) + "/" + blocks.Count; // Номер блока

        // Устанавливаем фоновое изображение
        if (background != null)
        {
            if (!string.IsNullOrEmpty(block.image) && block.image != "null")
            {
                background.Texture = block.image;
            }
            else
            {
                background.Texture = "data/maxim_batteryUp/ui/infoBackground.png"; // Возвращаем дефолтный фон
            }
        }

        // Устанавливаем PNG на позиции contentLabel
        if (textImage != null)
        {
            if (!string.IsNullOrEmpty(block.image2) && block.image2 != "null")
            {
                textImage.Texture = block.image2;
                textImage.Hidden = false; // Показываем PNG
            }
            else
            {
                textImage.Hidden = true; // Скрываем PNG, если image2 не задано
            }
        }
    }

    // Переход к следующему блоку
    private void NextBlock()
    {
        if (currentBlockIndex == blocks.Count - 1)
        {
            // Закрываем интерфейс и показываем главное меню
            if (mainMenu != null)
            {
                //mainMenu.ShowMenu(); // Показываем главное меню
            }
            else
            {
                Log.Error("UIMainMenu is not assigned.\n");
            }
            if (VBox != null)
                VBox.Hidden = true; // Скрываем интерфейс
        }
        else
        {
            currentBlockIndex++; // Переходим к следующему блоку
            ShowBlock(); // Показываем новый блок
        }
    }

    // Загрузка блоков из JSON-файла
    private void LoadBlocks()
    {
        // Проверяем, задан ли путь к JSON-файлу
        if (string.IsNullOrEmpty(jsonFile) || jsonFile == "null")
        {
            Log.Error("JSON file path is not set or invalid: {0}\n", jsonFile);
            return;
        }

        // Проверяем, успешно ли загружен JSON
        if (json.Load(jsonFile) == 0)
        {
            Log.Error("Failed to load JSON file: {0}\n", jsonFile);
            return;
        }

        jsonBlocks = json.GetChild("blocks"); // Получаем узел "blocks"
        if (jsonBlocks == null)
        {
            Log.Error("No 'blocks' node found in JSON file: {0}\n", jsonFile);
            return;
        }

        // Проходим по всем блокам в JSON
        for (int i = 0; i < jsonBlocks.GetNumChildren(); i++)
        {
            Json jsonBlock = jsonBlocks.GetChild(i);
            if (jsonBlock == null)
            {
                Log.Error("Invalid JSON block at index {0}\n", i);
                continue;
            }

            string title = jsonBlock.Read("title") ?? "No Title"; // Читаем заголовок
            string content = jsonBlock.Read("content") ?? "No Content"; // Читаем контент
            string image = jsonBlock.Read("image") ?? null; // Читаем путь к фоновому изображению
            string image2 = jsonBlock.Read("image2") ?? null; // Читаем путь к PNG на позиции contentLabel

            // Добавляем блок в список
            blocks.Add(new Block
            {
                title = title,
                content = content,
                image = image,
                image2 = image2
            });
        }
    }

    // Создание основного интерфейса
    private void CreateUI()
    {
        VBox = new WidgetVBox(); // Создаем вертикальный контейнер
        if (VBox == null)
        {
            Log.Error("Failed to create VBox.\n");
            return;
        }
        VBox.Width = 1920; // Полноэкранная ширина
        VBox.Height = 1080; // Полноэкранная высота

        background = new WidgetSprite(); // Фон
        if (background == null)
        {
            Log.Error("Failed to create background sprite.\n");
            return;
        }
        background.Width = 1920; // Полноэкранная ширина
        background.Height = 1080; // Полноэкранная высота
        background.Texture = "data/maxim_batteryUp/ui/infoBackground.png";
        background.SetPosition(0, 0);

        textImage = new WidgetSprite(); // PNG на позиции contentLabel
        if (textImage == null)
        {
            Log.Error("Failed to create textImage sprite.\n");
            return;
        }
        textImage.Width = 1800; // Соответствует ширине contentLabel
        textImage.Height = 600; // Соответствует высоте contentLabel
        textImage.SetPosition(150, 750); // Совпадает с contentLabel
        textImage.Hidden = true; // Изначально скрыто

        titleLabel = new WidgetLabel(); // Метка для заголовка
        if (titleLabel == null)
        {
            Log.Error("Failed to create titleLabel.\n");
            return;
        }
        titleLabel.Height = 120; // Увеличена высота для полноэкранного режима
        titleLabel.Width = 1800; // Ширина почти на весь экран
        titleLabel.Text = "Заголовок";
        titleLabel.SetFont("Arial"); // Восстановлено из вашего кода
        titleLabel.FontSize = 48; // Увеличен шрифт для читаемости
        titleLabel.SetPosition(60, 50); // Сдвинут ближе к верхнему краю
        titleLabel.FontColor = new vec4(1.0f, 1.0f, 1.0f, 1.0f); // Белый цвет
        titleLabel.FontWrap = 1; // Перенос текста

        contentLabel = new WidgetLabel(); // Метка для контента
        if (contentLabel == null)
        {
            Log.Error("Failed to create contentLabel.\n");
            return;
        }
        contentLabel.Height = 600; // Увеличена высота для большего текста
        contentLabel.Width = 1800; // Ширина почти на весь экран
        contentLabel.Text = "Контент";
        contentLabel.SetFont("Times New Roman"); // Восстановлено из вашего кода
        contentLabel.FontSize = 36; // Увеличен шрифт для читаемости
        contentLabel.SetPosition(200, 800); // Исходная позиция
        contentLabel.FontColor = new vec4(1.0f, 1.0f, 1.0f, 1.0f); // Белый цвет
        contentLabel.FontWrap = 1; // Перенос текста

        blockImage = new WidgetSprite(); // Изображение для блока
        if (blockImage == null)
        {
            Log.Error("Failed to create blockImage sprite.\n");
            return;
        }
        blockImage.Width = 600; // Увеличено для полноэкранного режима
        blockImage.Height = 300; // Увеличено для пропорций
        blockImage.SetPosition(660, 600); // Центрировано ниже контента
        blockImage.Hidden = true; // Изначально скрыто

        numberLabel = new WidgetLabel(); // Метка для номера блока
        if (numberLabel == null)
        {
            Log.Error("Failed to create numberLabel.\n");
            return;
        }
        numberLabel.Width = 150; // Увеличено для полноэкранного режима
        numberLabel.Height = 80; // Увеличено для читаемости
        numberLabel.Text = "1/4";
        numberLabel.FontSize = 48; // Увеличен шрифт
        numberLabel.SetPosition(885, 950); // Перемещен в нижнюю центральную часть
        numberLabel.FontColor = new vec4(1.0f, 1.0f, 1.0f, 1.0f); // Белый цвет
        numberLabel.FontWrap = 1;

        // Добавляем элементы в контейнер VBox
        VBox.AddChild(background, Gui.ALIGN_OVERLAP);
        VBox.AddChild(textImage, Gui.ALIGN_OVERLAP); // PNG на позиции contentLabel
        VBox.AddChild(titleLabel, Gui.ALIGN_OVERLAP);
        VBox.AddChild(contentLabel, Gui.ALIGN_OVERLAP);
        VBox.AddChild(blockImage, Gui.ALIGN_OVERLAP);
        //VBox.AddChild(numberLabel, Gui.ALIGN_OVERLAP); // Оставлено закомментированным

        // Добавляем контейнер в главное окно, центрируем
        WindowManager.MainWindow.AddChild(VBox, Gui.ALIGN_OVERLAP | Gui.ALIGN_CENTER);

        if (VBox != null)
            VBox.Hidden = true; // Изначально интерфейс скрыт
    }
}