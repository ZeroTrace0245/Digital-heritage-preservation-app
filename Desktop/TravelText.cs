using System.Collections.Generic;
using System.Linq;

namespace digital_heritage_preservation_app;

public static class TravelText
{
    // Stable English keys keep stored country/category values independent of the display language.
    private static readonly Dictionary<string, string[]> Text = new()
    {
        ["Discover"] = new[] { "見つける", "둘러보기", "Открыть мир" },
        ["Places wiki"] = new[] { "旅先ウィキ", "장소 위키", "Вики о местах" },
        ["Saved places"] = new[] { "保存した場所", "저장한 장소", "Сохранённые места" },
        ["My trips"] = new[] { "旅のプラン", "내 여행", "Мои поездки" },
        ["Settings"] = new[] { "設定", "설정", "Настройки" },
        ["Heritage archive"] = new[] { "文化遺産アーカイブ", "문화유산 기록", "Архив наследия" },
        ["Local"] = new[] { "地元", "내 지역", "Местные" },
        ["All"] = new[] { "すべて", "전체", "Все" },
        ["Sri Lanka"] = new[] { "スリランカ", "스리랑카", "Шри-Ланка" },
        ["Japan"] = new[] { "日本", "일본", "Япония" },
        ["South Korea"] = new[] { "韓国", "대한민국", "Южная Корея" },
        ["Russia"] = new[] { "ロシア", "러시아", "Россия" },
        ["+ Plan a trip"] = new[] { "+ 旅行を計画", "+ 여행 계획", "+ Создать поездку" },
        ["+ Add a place"] = new[] { "+ 場所を追加", "+ 장소 추가", "+ Добавить место" },
        ["Search destinations, regions or experiences"] = new[] { "目的地・地域・体験を検索", "여행지, 지역, 체험 검색", "Поиск мест, регионов и впечатлений" },
        ["All experiences"] = new[] { "すべての体験", "모든 체험", "Все впечатления" },
        ["Heritage"] = new[] { "文化遺産", "문화유산", "Наследие" },
        ["Nature"] = new[] { "自然", "자연", "Природа" },
        ["Coast"] = new[] { "海岸", "해안", "Побережье" },
        ["Culture"] = new[] { "文化", "문화", "Культура" },
        ["Find your kind of escape"] = new[] { "自分らしい旅を見つけよう", "나만의 여행을 찾아보세요", "Найдите своё путешествие" },
        ["Your personal shortlist"] = new[] { "あなたのお気に入り", "나만의 여행 목록", "Ваш список избранного" },
        ["Keep a little inspiration for later."] = new[] { "次の旅のヒントを保存しましょう。", "다음 여행을 위한 영감을 저장하세요.", "Сохраните вдохновение на потом." },
        ["Less organizing. More exploring."] = new[] { "計画はシンプルに。発見は豊かに。", "계획은 간단하게, 여행은 풍성하게.", "Меньше хлопот. Больше открытий." },
        ["A travel companion that feels like yours."] = new[] { "あなたに合った旅のパートナー。", "나에게 맞는 여행 동반자.", "Ваш личный спутник в путешествиях." },
        ["Choose a country. Find your next chapter."] = new[] { "国を選んで、次の旅へ。", "나라를 선택하고 다음 여행을 찾아보세요.", "Выберите страну для новой истории." },
        ["GO SOMEWHERE THAT STAYS WITH YOU"] = new[] { "心に残る場所へ", "오래 기억될 곳으로", "ТУДА, ГДЕ РОЖДАЮТСЯ ВОСПОМИНАНИЯ" },
        ["A world of wonder."] = new[] { "驚きに満ちた世界。", "경이로 가득한 세상.", "Мир удивительных открытий." },
        ["Explore featured place  →"] = new[] { "おすすめの場所へ  →", "추천 장소 둘러보기  →", "Открыть выбранное место  →" },
        ["Explore destination  ↗"] = new[] { "目的地を見る  ↗", "여행지 살펴보기  ↗", "Подробнее о месте  ↗" },
        ["No places found. Try another search or save a destination from Discover."] = new[] { "場所が見つかりません。検索条件を変えるか、場所を追加してください。", "장소가 없습니다. 검색 조건을 바꾸거나 장소를 추가하세요.", "Мест не найдено. Измените поиск или добавьте своё место." },
        ["Make room for the memorable."] = new[] { "忘れられない体験のために。", "기억에 남을 순간을 위해.", "Оставьте место незабываемому." },
        ["Your next adventure starts with a plan. Create a trip, then add destinations to each day."] = new[] { "旅行を作成して、各日に目的地を追加しましょう。", "여행을 만들고 날짜별로 여행지를 추가하세요.", "Создайте поездку и добавьте места на каждый день." },
        ["Your itinerary"] = new[] { "旅程", "여행 일정", "Ваш маршрут" },
        ["Edit trip"] = new[] { "旅行を編集", "여행 수정", "Изменить поездку" },
        ["Export itinerary"] = new[] { "旅程を書き出す", "일정 내보내기", "Экспорт маршрута" },
        ["Delete"] = new[] { "削除", "삭제", "Удалить" },
        ["Add places from Discover. Select a stop below to edit its day or notes."] = new[] { "場所を追加し、下の項目から日付やメモを編集できます。", "둘러보기에서 장소를 추가하세요. 아래 장소를 선택해 날짜와 메모를 수정하세요.", "Добавляйте места из обзора. Выберите остановку для изменения дня или заметок." },
        ["An open day. Add a destination or leave room to wander."] = new[] { "自由な一日。目的地を追加するか、気ままに過ごしましょう。", "자유로운 하루. 장소를 추가하거나 여유를 즐기세요.", "Свободный день. Добавьте место или оставьте время для прогулок." },
        ["Make yourself at home."] = new[] { "自分らしく設定しましょう。", "나에게 맞게 설정하세요.", "Настройте всё по-своему." },
        ["Appearance"] = new[] { "外観", "화면 모드", "Оформление" },
        ["Use system setting"] = new[] { "システム設定を使用", "시스템 설정 사용", "Как в системе" },
        ["Light"] = new[] { "ライト", "라이트", "Светлая" },
        ["Dark"] = new[] { "ダーク", "다크", "Тёмная" },
        ["App language"] = new[] { "アプリの言語", "앱 언어", "Язык приложения" },
        ["Home country / Local"] = new[] { "地元として表示する国", "내 지역으로 표시할 국가", "Страна для вкладки «Местные»" },
        ["Apply home country"] = new[] { "地元の国を保存", "내 지역 저장", "Сохранить страну" },
        ["Local uses your chosen home country, not GPS. Your last location tab is remembered."] = new[] { "地元には選択した国を表示します。GPSは使用しません。最後のタブを記憶します。", "내 지역은 GPS 대신 선택한 국가를 사용합니다. 마지막 탭이 저장됩니다.", "Вкладка «Местные» использует выбранную страну без GPS. Последняя вкладка сохраняется." },
        ["Destination descriptions and your own notes keep their original language."] = new[] { "目的地の説明とメモは元の言語で表示されます。", "여행지 설명과 메모는 원래 언어로 표시됩니다.", "Описания мест и ваши заметки сохраняют исходный язык." },
        ["Your journey, on your device"] = new[] { "あなたの旅を、このデバイスに", "내 기기에 저장되는 여행", "Ваши поездки на вашем устройстве" },
        ["Export travel backup"] = new[] { "旅行のバックアップを書き出す", "여행 백업 내보내기", "Экспорт резервной копии" },
        ["Restore travel backup"] = new[] { "旅行のバックアップを復元", "여행 백업 복원", "Восстановить резервную копию" },
        ["Photo credits"] = new[] { "写真のクレジット", "사진 출처", "Авторы фотографий" },
        ["Close"] = new[] { "閉じる", "닫기", "Закрыть" },
        ["Cancel"] = new[] { "キャンセル", "취소", "Отмена" },
        ["Save trip"] = new[] { "旅行を保存", "여행 저장", "Сохранить поездку" },
        ["Your next chapter"] = new[] { "次の旅へ", "다음 여행", "Ваша новая история" },
        ["Trip name"] = new[] { "旅行名", "여행 이름", "Название поездки" },
        ["Start date"] = new[] { "出発日", "출발일", "Дата начала" },
        ["Number of days (1–60)"] = new[] { "日数（1〜60）", "여행 일수 (1–60)", "Количество дней (1–60)" },
        ["Day"] = new[] { "日", "일차", "День" },
        ["Notes"] = new[] { "メモ", "메모", "Заметки" },
        ["Notes (optional)"] = new[] { "メモ（任意）", "메모 (선택 사항)", "Заметки (необязательно)" },
        ["Save stop"] = new[] { "予定を保存", "방문 일정 저장", "Сохранить остановку" },
        ["Remove stop"] = new[] { "予定を削除", "방문 일정 삭제", "Удалить остановку" },
        ["Add to a trip"] = new[] { "旅行に追加", "여행에 추가", "Добавить в поездку" },
        ["Add to itinerary"] = new[] { "旅程に追加", "일정에 추가", "Добавить в маршрут" },
        ["♥ Saved to your places"] = new[] { "♥ 保存済み", "♥ 저장한 장소", "♥ Место сохранено" },
        ["♡ Save this place"] = new[] { "♡ この場所を保存", "♡ 이 장소 저장", "♡ Сохранить место" },
        ["Open location in maps ↗"] = new[] { "地図で開く ↗", "지도에서 보기 ↗", "Открыть на карте ↗" },
        ["Customize place"] = new[] { "場所をカスタマイズ", "장소 맞춤 설정", "Настроить место" },
        ["Add your own place"] = new[] { "自分の場所を追加", "나만의 장소 추가", "Добавить своё место" },
        ["Save place"] = new[] { "場所を保存", "장소 저장", "Сохранить место" },
        ["Place name"] = new[] { "場所の名前", "장소 이름", "Название места" },
        ["Country"] = new[] { "国", "국가", "Страна" },
        ["Region / city"] = new[] { "地域・都市", "지역 / 도시", "Регион / город" },
        ["Experience"] = new[] { "体験", "체험", "Впечатление" },
        ["Description"] = new[] { "説明", "설명", "Описание" },
        ["Short highlight"] = new[] { "ひとこと紹介", "짧은 소개", "Краткое описание" },
        ["Reset to original"] = new[] { "元に戻す", "기본값으로 재설정", "Вернуть оригинал" },
        ["Delete this trip?"] = new[] { "この旅行を削除しますか？", "이 여행을 삭제할까요?", "Удалить эту поездку?" },
        ["Restore travel backup?"] = new[] { "バックアップを復元しますか？", "여행 백업을 복원할까요?", "Восстановить резервную копию?" },
        ["Restore"] = new[] { "復元", "복원", "Восстановить" },
        ["Enter a name, start date and a whole number of days from 1 to 60."] = new[] { "名前、出発日、1〜60の整数の日数を入力してください。", "이름, 출발일, 1~60 사이의 정수 일수를 입력하세요.", "Введите название, дату и целое число дней от 1 до 60." },
        ["Enter a whole day within this trip."] = new[] { "旅行期間内の整数の日を入力してください。", "여행 기간 내의 정수 일차를 입력하세요.", "Введите целый номер дня в пределах поездки." },
        ["Choose a trip and a whole day within its duration."] = new[] { "旅行と期間内の整数の日を選択してください。", "여행과 기간 내의 정수 일차를 선택하세요.", "Выберите поездку и целый номер дня в её пределах." },
        ["Move or remove later stops before shortening this trip."] = new[] { "旅行を短くする前に後半の予定を移動または削除してください。", "여행을 줄이기 전에 이후 일정을 옮기거나 삭제하세요.", "Перед сокращением поездки перенесите или удалите поздние остановки." },
        ["Export saved successfully."] = new[] { "書き出しました。", "내보내기가 완료되었습니다.", "Файл успешно сохранён." },
        ["Travel backup restored."] = new[] { "バックアップを復元しました。", "여행 백업이 복원되었습니다.", "Резервная копия восстановлена." },
        ["Home country saved."] = new[] { "地元の国を保存しました。", "내 지역을 저장했습니다.", "Страна сохранена." },
        ["Enter a country name (up to 80 characters)."] = new[] { "国名を80文字以内で入力してください。", "국가 이름을 80자 이내로 입력하세요.", "Введите название страны (до 80 символов)." },
        ["Enter a place name, country and description."] = new[] { "場所の名前、国、説明を入力してください。", "장소 이름, 국가, 설명을 입력하세요.", "Введите название места, страну и описание." },
        ["Made for a slower journey."] = new[] { "ゆっくりと楽しむ旅のために。", "여유로운 여행을 위해.", "Для неспешных путешествий." },
        ["Your saved places and plans stay on this device."] = new[] { "保存した場所と計画はこのデバイスに保管されます。", "저장한 장소와 계획은 이 기기에 보관됩니다.", "Места и планы хранятся на этом устройстве." },
        ["YOUR NEXT CHAPTER"] = new[] { "次の旅へ", "다음 여행", "ВАША НОВАЯ ИСТОРИЯ" },
        ["CONNECTED TO CULTURE"] = new[] { "文化とつながる", "문화와 함께", "ПРИКОСНИТЕСЬ К КУЛЬТУРЕ" }
    };
    public static string Get(string key, string language)
    {
        var index = language switch { "ja" => 0, "ko" => 1, "ru" => 2, _ => -1 };
        return index >= 0 && Text.TryGetValue(key, out var translations) ? translations[index] : key;
    }
    public static string TranslateDisplayed(string value, string language)
    {
        var key = Text.ContainsKey(value) ? value : Text.FirstOrDefault(p => p.Value.Contains(value)).Key;
        return key is null ? value : Get(key, language);
    }
}
