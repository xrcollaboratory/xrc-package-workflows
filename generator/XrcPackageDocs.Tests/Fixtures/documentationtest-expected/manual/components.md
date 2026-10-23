# Components

This document lists all MonoBehaviour components in the package.

## Documentation Test Feedback

Shows the test area of `DocumentationTestLogic`.

A second paragraph, to check that paragraphs survive. The area is drawn as a wire sphere with `DocumentationTestFeedback.color`.

![Documentation Test Feedback](../images/components/DocumentationTestFeedback.png)

| Property | Description |
|----------|-------------|
| **Color** | The color of the test area. Changing it takes effect the next time the area is drawn. |

## Documentation Test Logic

The logic of the documentation test package. It covers the cases a components page has to handle: a `sealed` class, an inherited field (`DocumentationTestBase.speed`), properties named differently from their fields, and fields documented in different ways.

![Documentation Test Logic](../images/components/DocumentationTestLogic.png)

| Property | Description |
|----------|-------------|
| **Speed** | How fast the component works, in units per second. Inherited by every documentation test component, so its description has to come from this base class. |
| **Radius** | The radius of the test area, in meters. The property is named differently from its field (`m_Radius`), so the description must be found through the property's getter, not its name. |
| **Target** | The transform the test area follows. When `null`, the area stays where it is. |
| **Tags** | Tags for the objects inside the area. Objects with any of these tags are counted: <ul><li>an empty list counts every object;</li><li>tags are matched exactly.</li></ul> |
| **Capacity** | How many objects the area may hold. This field has no public property, so its own summary describes it. |
| **Log Entries** | *No description provided* |
