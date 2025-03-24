using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Plant;

public class Axe
{
    private PlayerController control;
    private PlantingManager plantingManager;
    private List<Vector3> sowedPlots;

    public Axe(PlayerController control, PlantingManager plantingManager, List<Vector3> sowedPlots)
    {
        this.control = control;
        this.plantingManager = plantingManager;
        this.sowedPlots = sowedPlots;
    }

    public void Execute(Tool toolUsed, Vector3 position)
    {
        int plantIndex = sowedPlots.IndexOf(position);
        if (plantIndex >= 0)
        {
            var plant = plantingManager.sowedPlants[plantIndex];
            plant.Durability -= toolUsed.efficiency;
            if (plant.Durability <= 0) 
            {
                var treeProduct = plant.GetProduct(ProductType.Material);
                control.StartCoroutine(TreeDown(plantIndex, treeProduct));
            }
        }
    }

    private IEnumerator TreeDown(int plantIndex, ProductModifier treeProduct)
    {
        var plant = plantingManager.sowedPlants[plantIndex];
        int currentStageIndex = plant.CurrentStageIndex;

        string animation = "falldown_" + GetPosition(control.transform, plant.CurrentGameObject.transform);
        Animator animator = plant.CurrentGameObject.GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play(animation);

            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(animation));

            float distanceFall = GetPosition(control.transform, plant.CurrentGameObject.transform) == "right" ? 0.25f : -0.25f;
            float gapX = plant.CurrentGameObject.transform.position.x + distanceFall;
            float gapY = plant.CurrentGameObject.transform.position.y;
            plant.CurrentGameObject.transform.position = new(gapX, gapY);
        }


        yield return new WaitWhile(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.6f);

        for(int i = 0; i < treeProduct.quantity; i++)
        {
            CircleCollider2D circleCollider = new CircleCollider2D();
            if(plant.CurrentGameObject.GetComponentInChildren<CircleCollider2D>() != null)
            {
                circleCollider = plant.CurrentGameObject.GetComponentInChildren<CircleCollider2D>();

                Vector2 initialPosition = circleCollider.transform.position;
                Vector2 targetPosition = GetRandomPositionCircle(circleCollider);

                GameObject newItem = ToolUsedManager.Instantiate(treeProduct.product.gameObj, initialPosition, Quaternion.identity);
                control.StartCoroutine(SlidingEffect(newItem.transform, targetPosition, 1f));
            }
        }
        
        yield return new WaitWhile(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1f);

        if (treeProduct != null)
        {

            if (treeProduct.harvestType == HarvestType.MultipleProduction)
            {
                plantingManager.RegrowPlant(plant);
            }
        }
        if (plant != null)
        {
            plantingManager.RemovePlant(plantIndex);
        }
    }

    private string GetPosition(Transform player, Transform tree)
    {
        string pos = player.position.x < tree.position.x ? "right" : "left";
        if (player.position.x == tree.position.x)
        {
            pos = Random.Range(0, 2) == 0 ? "right" : "left";
        }
        return pos;
    }

    Vector2 GetRandomPositionCircle(CircleCollider2D circleCollider)
    {
        Vector2 center = circleCollider.transform.position;
        float radius = circleCollider.radius;

        float randomAngle = Random.Range(0f, 360f);
        float angleRadian = randomAngle * Mathf.Deg2Rad;

        float randomDistance = Random.Range(0F, radius);

        float x = center.x + randomDistance * Mathf.Cos(angleRadian);
        float y = center.y + randomDistance * Mathf.Sin(angleRadian);

        return new Vector2(x, y);
    }

    IEnumerator SlidingEffect(Transform itemTransform, Vector2 targetPosition, float duration)
    {
        Vector2 startPosition = itemTransform.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            itemTransform.position = Vector2.Lerp(startPosition, targetPosition, elapsedTime / duration);
            itemTransform.GetComponent<CircleCollider2D>().enabled = elapsedTime >= duration;

            yield return null;
        }
    }
}
