using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace digital_heritage_preservation_app;

public sealed partial class TourismWindow
{
    private bool guideOpen;

    private async void InvokeNavigationItem(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (ready && args.InvokedItemContainer?.Tag?.ToString() == "Guide") await ShowUserGuide();
    }

    private async Task ShowUserGuide()
    {
        if (guideOpen) return;
        guideOpen = true;
        try
        {
            var content = new StackPanel { Spacing = 18, MaxWidth = 500 };
            content.Children.Add(new TextBlock
            {
                Text = data.Language switch { "ja" => "Loreviaへようこそ。場所と物語を発見し、旅を計画しましょう。", "ko" => "Lorevia에 오신 것을 환영합니다. 장소와 이야기를 발견하고 여행을 계획하세요.", "ru" => "Добро пожаловать в Lorevia! Открывайте места и их истории, планируйте путешествия.", _ => "Welcome to Lorevia! Discover places, learn their stories and plan your journey in one app." },
                TextWrapping = TextWrapping.Wrap, FontSize = 18
            });
            var sections = GuideSections();
            foreach (var section in sections)
            {
                var block = new StackPanel { Spacing = 6 };
                block.Children.Add(new TextBlock { Text = section.Title, FontSize = 18, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap });
                block.Children.Add(new TextBlock { Text = section.Text, TextWrapping = TextWrapping.Wrap });
                content.Children.Add(block);
            }
            var dialog = Dialog("User guide", content, "");
            dialog.CloseButtonText = T("Close");
            dialog.DefaultButton = ContentDialogButton.Close;
            await ShowDialog(dialog);
            if (!data.HasSeenUserGuide && writable) Change(next => next.HasSeenUserGuide = true);
        }
        catch (Exception)
        {
            Message("The user guide could not open. Try User guide at the bottom of the sidebar.", true);
        }
        finally { guideOpen = false; }
    }

    private (string Title, string Text)[] GuideSections() => data.Language switch
    {
        "ja" => new[]
        {
            ("1. 場所を見つける", "上部で国を選びます。「すべて」は全地域、「地元」は設定した国です。検索と体験フィルターで場所を絞り、カードを開いて詳細を読みます。"),
            ("2. 保存とカスタマイズ", "場所を保存して後で見返したり、説明・歴史・アクセシビリティ・出典を編集したりできます。自分の場所も追加できます。出典の確認日を記録しましょう。"),
            ("3. ウィキとオフライン記事", "旅先ウィキで場所を検索します。Wikipediaの記事はアプリ内で開き、「記事をダウンロード」でテキストを保存できます。オフラインライブラリで検索・閲覧・削除ができます。画像は保存されません。"),
            ("4. 旅程と予算", "旅行名・開始日・日数・通貨コード・予算を入力します。場所を旅行の日程に追加し、予定の編集で費用を入力します。上下ボタンで同じ日の順番を変更できます。PDF保存と印刷にも対応します。"),
            ("5. 共有とクラウド同期", "旅行を共有するとファイルを書き出せます。共有旅行の読み込みは新しい旅行を作成します。設定でOneDriveやDropboxの同期済みフォルダーを接続すると、各端末が別々の保存版を作ります。受信した保存版は確認後に適用してください。"),
            ("6. 文化遺産と復元", "文化遺産アーカイブは別ウィンドウで物語や資料を保管します。旅行とアーカイブの直近10バージョンを自動保存します。設定の復元用バックアップから戻せます。クラウド同期にはアーカイブやダウンロード記事は含まれません。"),
            ("7. 設定とサポート", "設定で言語・外観・地元の国を変更し、閲覧データを削除できます。アプリ情報とサポートから問題を報告したり更新を確認したりできます。このガイドは左下からいつでも開けます。")
        },
        "ko" => new[]
        {
            ("1. 장소 찾기", "위에서 국가를 선택하세요. '전체'는 모든 지역, '내 지역'은 설정한 국가입니다. 검색과 체험 필터로 장소를 찾고 카드를 열어 자세히 읽으세요."),
            ("2. 저장과 맞춤 설정", "장소를 저장해 다시 보거나 설명, 역사, 접근성, 출처를 편집할 수 있습니다. 나만의 장소도 추가하세요. 출처를 확인한 날짜를 기록할 수 있습니다."),
            ("3. 위키와 오프라인 글", "장소 위키에서 검색하세요. 위키백과 글은 앱 안에서 열리며 '글 다운로드'로 텍스트를 저장할 수 있습니다. 오프라인 보관함에서 검색, 읽기, 삭제가 가능합니다. 이미지는 저장되지 않습니다."),
            ("4. 일정과 예산", "여행 이름, 시작 날짜, 기간, 통화 코드와 예산을 입력하세요. 장소를 원하는 날짜에 추가하고 일정 편집에서 예상 비용을 입력하세요. 위아래 버튼으로 같은 날의 순서를 바꿀 수 있습니다. PDF 저장과 인쇄도 가능합니다."),
            ("5. 공유와 클라우드 동기화", "여행 공유로 파일을 내보내세요. 공유 여행을 가져오면 새 여행이 추가됩니다. 설정에서 OneDrive나 Dropbox가 동기화하는 폴더를 연결하면 기기마다 별도의 저장본을 만듭니다. 받은 저장본은 검토 후 적용하세요."),
            ("6. 문화유산과 복구", "문화유산 기록은 별도 창에서 이야기와 자료를 보관합니다. 여행과 기록의 최근 10개 버전이 자동 보관됩니다. 설정의 복구 백업에서 복원할 수 있습니다. 클라우드 동기화에는 기록과 다운로드한 글이 포함되지 않습니다."),
            ("7. 설정과 지원", "설정에서 언어, 외관, 내 지역 국가를 바꾸고 검색 데이터를 삭제하세요. 앱 정보 및 지원에서 문제를 신고하고 업데이트를 확인할 수 있습니다. 이 안내는 왼쪽 아래에서 언제든 다시 열 수 있습니다.")
        },
        "ru" => new[]
        {
            ("1. Найдите место", "Выберите страну сверху. «Все» показывает все страны, «Местные» — страну из настроек. Используйте поиск и фильтр впечатлений, затем откройте карточку места."),
            ("2. Сохраните и дополните", "Сохраняйте места и редактируйте описание, историю, доступность и источник. Можно добавить собственное место и указать дату проверки источника."),
            ("3. Вики и офлайн-статьи", "Ищите места в вики. Статьи Википедии открываются в приложении. «Скачать статью» сохраняет текст для офлайн-библиотеки, где можно искать, читать и удалять загрузки. Изображения не сохраняются."),
            ("4. Маршрут и бюджет", "Создайте поездку с названием, датой, длительностью, кодом валюты и бюджетом. Добавляйте места по дням и указывайте расходы при редактировании остановки. Кнопки выше и ниже меняют порядок в пределах дня. Доступны PDF и печать."),
            ("5. Обмен и облако", "Поделитесь поездкой через файл. Импорт создаёт новую поездку. Подключите в настройках папку, которую уже синхронизирует OneDrive или Dropbox. Каждое устройство сохраняет отдельную копию; перед применением просмотрите входящие данные."),
            ("6. Наследие и восстановление", "Архив наследия открывается в отдельном окне. Последние 10 версий планов и архива сохраняются автоматически. Восстановите их через настройки. Архив и загруженные статьи не входят в облачную синхронизацию поездок."),
            ("7. Настройки и помощь", "Меняйте язык, оформление и домашнюю страну в настройках, очищайте данные браузера. В разделе поддержки можно сообщить о проблеме и проверить обновления. Это руководство доступно внизу слева.")
        },
        _ => new[]
        {
            ("1. Discover places", "Choose a country above. All shows every country; Local uses your home country in Settings. Search and filter by experience, then open a place card."),
            ("2. Save and personalize", "Save places for later or customize their description, history, accessibility notes and source. Add your own destinations and record when you checked their source."),
            ("3. Wiki and offline reading", "Search Places wiki and read Wikipedia inside Lorevia. Download article saves its text to Offline library, where you can search, read and delete downloads. Images are not downloaded."),
            ("4. Itineraries and budgets", "Plan a trip with a name, start date, duration, currency code and budget. Add destinations to each day; edit a stop to enter estimated costs. Move up or down reorders stops within their day. Save PDF or Print creates a printable itinerary."),
            ("5. Sharing and cloud folders", "Share trip exports a file; Import shared trip creates a new trip without replacing existing places. Connect a folder already synced by OneDrive or Dropbox in Settings. Each device writes its own snapshot. Review incoming snapshots before replacing local plans."),
            ("6. Heritage and recovery", "Heritage archive opens its own window for stories and cultural records. The last 10 versions of travel and archive files are kept automatically. Use Recovery backups in Settings to restore them. Archive records and offline articles are separate from travel cloud sync."),
            ("7. Settings and support", "Change language, appearance and home country in Settings. Clear browsing data removes website sign-ins and cache. About and support includes feedback and update checks. Reopen this guide anytime from the bottom-left sidebar.")
        }
    };
}
