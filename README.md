# BehaviourMachine
`BehaviourMachine(BM)` - Это сочетание двух подходов - Машины состояний (Finite State Machine) и Дерева поведений (Behaviour Tree). Отсюда и название `BehaviourMachine`. `BM` позволяет связывать поведения как через код, так и визуально - с помощью GameObject'ов на сцене. С ее помощью можно легко оживить NPC или обрабатывать состояния каких-либо систем.
Позволяет обрабатывать состояния иерархично и/или параллельно. При желании можно легко написать собственный обработчик.
Общая схема графа:

![image](https://github.com/fdick/BehaviourMachine/assets/62177084/e8f9e39c-25d2-44e3-bee9-08b7c37f66e4)


## Производительность

## Быстрый старт
1. Создайте пустой `GameObject` на сцене. Для удобства переименнуйте его в `BehaviourMachine`.
2. Добавьте на него компонент `BehaviourMachine`. `Add Component` --> `BehaviourMachine`.
   
   ![image](https://github.com/fdick/AIBehaviourGraph/assets/62177084/ab41b217-05b2-4eff-bd11-1e1b56fd20c2)
   
3. Поставьте галочку `StartGraphOnStart` и нажмите на кнопку `Init Visualized Root Tree`.
   
   Автоматически создастся новый `VisualizedRootTree` с прикрепленным компонентом `VisualizedEmptyHierarchyTree`, который также автоматически заполнит собой ссылку в    `BehaviourMachine.cs`.

   ![image](https://github.com/fdick/AIBehaviourGraph/assets/62177084/a92de7e8-6bbb-4923-9912-bcaabbd20d3f)

4. Теперь следует заполнить "Листья" и "Связи" между ними. Чтобы добавить пустышки для "Листьев" нажмите `Add Leaf`, чтобы создать пустышку "Связь" нажмите `Add Link`.
5. После создания пустышек, следует создать свои "Листья" и "Линки" и назначить их на пустышки. После этого проставьте их в компонент `VisualizedEmptyHierarchyTree`. Укажите с какого "Листа" дереву следует начать `StartableLeaf_ID` и для удобства можно указать `Friendly Name` (Это поле нужно для Debug'а).

   ![image](https://github.com/fdick/AIBehaviourGraph/assets/62177084/10d51e9c-56d9-41b6-bb3d-20a71d80ed8c)

Простое дерево состояний готово. Чтобы узнать больше - читайте ниже.  
