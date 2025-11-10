using System.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class BlockSpawner : MonoBehaviour
{
    // GAMEOBJECT REFERENCES
    [SerializeField] GameObject chunk1;
    [SerializeField] GameObject chunk2;
    [SerializeField] GameObject blockPrefab;
    [SerializeField] GameObject activeChunk;

    // VECTOR POSITIONS
    Vector2 blockStartPos;
    Vector2 chunkStartPos;
    Vector2 chunkGameRdyPos;

    // VALUES
    [SerializeField] int rowQuantity;
    [SerializeField] int colQuantity;
    float blockHorizontalOffset;
    float blockVerticalOffset;
    [SerializeField] float chunkSpeed;
    [SerializeField] int blockAmount;

    private void Start()
    {
        blockHorizontalOffset = 2.5f;
        blockVerticalOffset = 1.28f;

        chunkStartPos = new Vector2(0, 15);
        chunkGameRdyPos = new Vector2(0, 6);
        blockStartPos = new Vector2(-12.5f, 15);

        chunk1.transform.position = chunkStartPos;
        chunk2.transform.position = chunkStartPos;

        if (blockPrefab == null)
        {
            Debug.Log("No blockprefab for instantiation");
        }

        activeChunk = chunk1;

        GetNewBlocks();
    }

    public void DestroyBlock(GameObject hitBlock)
    {
        Destroy(hitBlock);
        blockAmount--;
        
        if (blockAmount == 0)
        {
            GetNewBlocks();
        }
    }

    private GameObject SpawnBlocks()
    {
        blockAmount = rowQuantity * colQuantity;

        GameObject currentChunk = null;

        if (chunk1.tag != "Active")
        {
            currentChunk = chunk2;
            chunk1.tag = "Active";
            chunk1.transform.position = chunkStartPos;

        } else {
            currentChunk = chunk1;
            chunk1.tag = "Untagged";
            chunk2.transform.position = chunkStartPos;
        }

        Vector2 spawnPos = blockStartPos;

        for (int col = 0; col < colQuantity; ++col)
        {
            spawnPos = blockStartPos;
            spawnPos.y += -blockVerticalOffset * col;
            for (int row = 0; row < rowQuantity; ++row)
            {
                // Instantiates the blocks as a child of the currentChunk
                Instantiate(blockPrefab, spawnPos, currentChunk.transform.rotation, currentChunk.transform);
                spawnPos.x += blockHorizontalOffset;
            }
        }
        return currentChunk;
    }

    public void GetNewBlocks() 
    {
        // If the current chunk isn't depopulated before creating a new one, delete each block
        if (activeChunk.transform.childCount != 0)
        {
            for (int i = 0; i < activeChunk.transform.childCount; ++i)
            {
                var currentChild = activeChunk.transform.GetChild(i);
                Destroy(currentChild.gameObject);
            }
        }

        activeChunk = SpawnBlocks();
        StartCoroutine(Lerp(activeChunk));
    }

    IEnumerator Lerp(GameObject currentChunk)
    {
        float timeElapsed = 0;

        while (timeElapsed < chunkSpeed)
        {
            currentChunk.transform.position = Vector2.Lerp(chunkStartPos, chunkGameRdyPos, (timeElapsed / chunkSpeed)); 
            timeElapsed += Time.deltaTime;

            yield return null;
        }

       currentChunk.transform.position = chunkGameRdyPos;
    }
}