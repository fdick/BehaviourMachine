# AIBehaviourGraph
`AIBehaviourGraph` - Это машина состояний основанная на GameObject'ах. С ее помощью можно легко оживить NPC или обрабатывать состояния каких-либо систем.

## Производительность

## Быстрый старт
1. Создайте пустой `GameObject` на сцене. Для удобства переименнуйте его в `AIBehaviourGraph`.
2. Добавьте на него компонент `AIBehaviourGraph`. `Add Component` --> `AIBehaviourGraph`.
   
   ![image](https://github.com/fdick/AIBehaviourGraph/assets/62177084/ab41b217-05b2-4eff-bd11-1e1b56fd20c2)
   
4. Поставьте галочку `StartGraphOnStart` и нажмите на кнопку `Init Visualized Root Tree`.
   
Автоматически создастся новый `VisualizedRootTree` с прикрепленным компонентом `VisualizedEmptyHierarchyTree`, который также автоматически заполнит собой ссылку в `AiBehaviourGraph.cs`.  
   ![image](https://github.com/fdick/AIBehaviourGraph/assets/62177084/a92de7e8-6bbb-4923-9912-bcaabbd20d3f)
