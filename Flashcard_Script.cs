using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;
using System.Linq;

//Each math question is constructed using the Question object
//Each question has 2 integers, a difficulty, and a consecutive correct.
//There is a constructor and a function that calculates the starting difficulty -- can be modified based on preference
public class Question
{
    private int _intA;

    public int IntA
    {
        get => _intA;
        set
        {
            _intA = value;
        }
    }

    private int _intB;

    public int IntB
    {
        get => _intB;
        set
        {
            _intB = value;
        }
    }

    private int _difficulty;

    public int Difficulty
    {
        get => _difficulty;
        set
        {
            _difficulty = value;
        }
    }

    private int _consecutiveCorrect;

    public int ConsecutiveCorrect
    {
        get => _consecutiveCorrect;
        set
        {
            _consecutiveCorrect = value;
        }
    }

    public Question(int intA, int intB)
    {
        IntA = intA;
        IntB = intB;
        ConsecutiveCorrect = 0;
        Difficulty = CalculateDifficulty();
    }

    public int CalculateDifficulty()
    {
        if (IntA == 1 || IntB == 1 || IntA == 0 || IntB == 0)
        {
            return 2;
        }
        else if (IntA == 2 || IntB == 2 || (IntA == 5 && IntB == 3) || (IntA == 5 && IntB == 4) || (IntA == 3 && IntB == 5) || (IntA == 4 && IntB == 5))
        {
            return 3;
        }
        else if (IntA == IntB || IntA + IntB == 10)
        {
            return 4;
        }
        else if (IntA == 9 || IntB == 9 || IntA + IntB == 11)
        {
            return 5;
        }
        else
        {
            return 6;
        }
        //http://www.donaldsauter.com/single-digit-addition.htm
    }
    
}

//The questions are stored in the QuestionBank object
//The questionBank has 2 lists that store each question and the current difficulty of each question
//The questionBank has a totalDifficulty that is the sum of the current difficulty list
//The questionBank stores the previously asked question
//The questionBank takes in a max integer as a constructor. That integer will be the largest number asked in questions
//The questionBank has a getRandomQuestion function that returns a random question based on the weight by difficulty
//Questions with more difficulty will be asked more often, but a question should never repeat
public class QuestionBank
{
    public List<Question> questionArray = new List<Question>();
    public List<int> difficulties = new List<int>();

    public int TotalDifficulty
    {
        get => difficulties.Sum();
    }

    private int _prevIndex;

    public int PrevIndex
    {
        get => _prevIndex;
        set
        {
            _prevIndex = value;
        }
    }

    private int _max;
    public int Max
    {
        get => _max;
        set
        {
            _max = value;
        }
    }

    public QuestionBank(int max) {
        for (int i = 0; i < max+1; i++)
        {
            for (int j = i; j < max+1; j++)
            {
                questionArray.Add(new Question(i, j));
            }
        }

        foreach(Question q in questionArray)
        {
            difficulties.Add(q.Difficulty);
        }
        Max = max;
    }

    public int GetRandomQuestion() {
        System.Random rnd = new System.Random();
        int random = rnd.Next(0, TotalDifficulty);
        List<int> weights = new List<int>();
        for (int i = 0; i < difficulties.Count; i++)
        {
            for (int j = 0; j < difficulties[i]; j++)
            {
                weights.Add(i);
            }
        }
        if (!(weights.IndexOf(PrevIndex) == 0 && weights.LastIndexOf(PrevIndex) == weights.Count - 1))
        {
            Debug.Log("Random is " + random);
            PrevIndex = weights[random];
            return weights[random];
        }
        else
        {
            int currIndex = PrevIndex;
            while (currIndex == PrevIndex)
            {
                currIndex = rnd.Next(0, ((Max+1)*(Max+2))/2);
            }
            PrevIndex = currIndex;
            return currIndex;
        }
    }
}

public class FlashcardScript : MonoBehaviour
{
    //Background
    public GameObject startScreen;

    //Flashcard shape, shape operations, text, question
    public RectTransform rShape;
    public GameObject questionBox;
    public Text questionText;
    private float transitionTime = 0.5f;
    private int isShrinking = -1; //-1 = get smaller, 1 = get bigger
    private bool isFlipping = false;
    private int cardNum = 0; //For Question 1, Question 2 etc. text
    private float distancePerTime;
    private float timeCount = 0;
    private int index; //index of question in questionBank generated

    public int random;

    private QuestionBank questionArray; //list of all questions

    //input fields

    //input for answering questions
    public GameObject answerInput;
    public int? input;
    public Text inputFieldText;
    public GameObject noAnswerText;

    //input for starting: the max number to use in questions
    public GameObject starterInput;
    public int input2;

    //helper objects
    //used in assistant mode to make addition easier
    public GameObject leftHelper0;
    public GameObject leftHelper1;
    public GameObject leftHelper2;
    public GameObject leftHelper3;
    public GameObject leftHelper4;
    public GameObject leftHelper5;
    public GameObject leftHelper6;
    public GameObject leftHelper7;
    public GameObject leftHelper8;
    public GameObject leftHelper9;

    public GameObject rightHelper0;
    public GameObject rightHelper1;
    public GameObject rightHelper2;
    public GameObject rightHelper3;
    public GameObject rightHelper4;
    public GameObject rightHelper5;
    public GameObject rightHelper6;
    public GameObject rightHelper7;
    public GameObject rightHelper8;
    public GameObject rightHelper9;

    private List<GameObject> leftHelpers = new List<GameObject>();
    private List<GameObject> rightHelpers = new List<GameObject>();

    //buttons

    //moves to next question from answer page
    public GameObject nextQuestionButton;

    //goes to answer page from question page
    public GameObject checkAnswerButton;
    public GameObject skipQuestionButton;

    //goes to question page from beginning page
    public GameObject beginButton;
    public GameObject assistantModeButton;
    private bool assistantMode;

    //goes to beginning page from any page, resets everything
    public GameObject restartButton;

    //visual elements
    public GameObject ArmsDownHappy;
    public GameObject ArmsUpHappy;
    public GameObject ArmsDownSad;
    public GameObject HelperModeSprite;

    //for getting all questions right in the end
    private bool celebration;
    private bool goingUp;

    //takes in the answer to the question as a string, it is checked for int later
    String tempInput;
    public void ScanIn(String? s)
    {
        tempInput = s;
        Debug.Log(tempInput);
    }

    //takes in the max number in questions as a string, it is checked for int later
    String tempInput2;
    public void ScanInStart(String? s)
    {
        tempInput2 = s;
        Debug.Log(tempInput2);
    }

    //resets all fields/positions/objects when restart button is clicked
    public void resetClicked()
    {
        cardNum = 0;

        if (assistantMode && answerInput)
        {
            assistantMode = false;
        }

        starterInput.SetActive(true);
        noAnswerText.SetActive(false);
        nextQuestionButton.SetActive(false);
        startScreen.SetActive(true);
        beginButton.SetActive(true);
        assistantModeButton.SetActive(true);

        questionBox.SetActive(false);
        answerInput.SetActive(false);
        checkAnswerButton.SetActive(false);
        skipQuestionButton.SetActive(false);

        ArmsDownHappy.SetActive(true);
        ArmsUpHappy.SetActive(true);
        ArmsDownSad.SetActive(false);
        HelperModeSprite.SetActive(false);

        for (int i = 0; i < 10; i++)
        {
            leftHelpers[i].SetActive(false);
        }
        for (int i = 0; i < 10; i++)
        {
            rightHelpers[i].SetActive(false);
        }

        restartButton.SetActive(false);

        celebration = false;

        distancePerTime = rShape.localScale.x / transitionTime; 
    }

    //begins the quiz after begin button is clicked. Checks if the max number is valid first
    public void BeginClicked()
    {
        try
        {
            input2 = int.Parse(tempInput2);
        }
        catch (FormatException)
        {
            noAnswerText.SetActive(true);
            Debug.Log("Error caught");
            return;
        }
        catch (ArgumentNullException)
        {
            noAnswerText.SetActive(true);
            Debug.Log("Error caught");
            return;
        }

        questionArray = new QuestionBank(input2);

        starterInput.SetActive(false);
        noAnswerText.SetActive(false);
        nextQuestionButton.SetActive(false);
        startScreen.SetActive(false);
        beginButton.SetActive(false);
        assistantModeButton.SetActive(false);

        ArmsDownHappy.SetActive(false);
        ArmsUpHappy.SetActive(false);

        questionBox.SetActive(true);
        answerInput.SetActive(true);
        checkAnswerButton.SetActive(true);
        skipQuestionButton.SetActive(true);

        restartButton.SetActive(true);

        assistantMode = false;

        index = questionArray.GetRandomQuestion();
        System.Random rnd = new System.Random();
        random = rnd.Next(0, 2);
        switch (random)
        {
            case 0:
                questionText.text = "Question " + (cardNum + 1) + ":\nWhat is\n" + questionArray.questionArray[index].IntA + " + " + questionArray.questionArray[index].IntB;
                break;
            case 1:
                questionText.text = "Question " + (cardNum + 1) + ":\nWhat is\n" + questionArray.questionArray[index].IntB + " + " + questionArray.questionArray[index].IntA;
                break;
        }
    }

    //begins the quiz in assistant mode after assistant mode button is clicked. Checks if the max number is valid first
    public void AssistantModeClicked()
    {
        try
        {
            input2 = Int32.Parse(tempInput2);
        }
        catch (FormatException)
        {
            noAnswerText.SetActive(true);
            Debug.Log("Error caught");
            return;
        }
        catch (ArgumentNullException)
        {
            noAnswerText.SetActive(true);
            Debug.Log("Error caught");
            return;
        }

        questionArray = new QuestionBank(input2);

        starterInput.SetActive(false);
        noAnswerText.SetActive(false);
        nextQuestionButton.SetActive(false);
        startScreen.SetActive(false);
        beginButton.SetActive(false);
        assistantModeButton.SetActive(false);

        questionBox.SetActive(true);
        answerInput.SetActive(true);
        checkAnswerButton.SetActive(true);
        skipQuestionButton.SetActive(true);

        restartButton.SetActive(true);

        assistantMode = true;

        ArmsDownHappy.SetActive(false);
        ArmsUpHappy.SetActive(false);

        index = questionArray.GetRandomQuestion();
        System.Random rnd = new System.Random();
        random = rnd.Next(0, 2);
        switch (random)
        {
            case 0:
                questionText.text = "Question " + (cardNum + 1) + ":\nWhat is\n" + questionArray.questionArray[index].IntA + " + " + questionArray.questionArray[index].IntB;
                break;
            case 1:
                questionText.text = "Question " + (cardNum + 1) + ":\nWhat is\n" + questionArray.questionArray[index].IntB + " + " + questionArray.questionArray[index].IntA;
                break;
        }

        if (input2 <= 9)
        {
            HelperModeSprite.SetActive(true);

            switch (random)
            {
                case 0:
                    for (int i = 0; i <= questionArray.questionArray[index].IntA; i++)
                    {
                        leftHelpers[i].SetActive(true);
                    }
                    for (int i = 0; i <= questionArray.questionArray[index].IntB; i++)
                    {
                        rightHelpers[i].SetActive(true);
                    }
                    break;
                case 1:
                    for (int i = 0; i <= questionArray.questionArray[index].IntB; i++)
                    {
                        leftHelpers[i].SetActive(true);
                    }
                    for (int i = 0; i <= questionArray.questionArray[index].IntA; i++)
                    {
                        rightHelpers[i].SetActive(true);
                    }
                    break;
            }
        }
    }

    //goes to the answer page from the question page when skip question button is clicked. Question is marked as incorrect. 
    public void SkipAnswerClicked()
    {
        questionArray.questionArray[index].ConsecutiveCorrect = 0;
        questionArray.questionArray[index].Difficulty++;
        questionArray.difficulties[index]++;

        timeCount = 0;
        answerInput.SetActive(false);
        checkAnswerButton.SetActive(false);
        noAnswerText.SetActive(false);
        skipQuestionButton.SetActive(false);
        isFlipping = true;

        for (int i = 0; i < 10; i++)
        {
            leftHelpers[i].SetActive(false);
        }
        for (int i = 0; i < 10; i++)
        {
            rightHelpers[i].SetActive(false);
        }

        ArmsDownSad.SetActive(true);
        HelperModeSprite.SetActive(false);

        switch (random)
        {
            case 0:
                questionText.text = questionArray.questionArray[index].IntA + " + " + questionArray.questionArray[index].IntB + " equals\n\n"
            + (questionArray.questionArray[index].IntA + questionArray.questionArray[index].IntB);
                break;
            case 1:
                questionText.text = questionArray.questionArray[index].IntB + " + " + questionArray.questionArray[index].IntA + " equals\n\n"
            + (questionArray.questionArray[index].IntA + questionArray.questionArray[index].IntB);
                break;
        }
    }

    //goes to the answer page from the question page when check answer button is clicked. Question is marked as correct. 
    public void CheckAnswerClicked()
    {
        timeCount = 0;
        Debug.Log("pressed");

        try
        {
            input = Int32.Parse(tempInput);
        }
        catch (FormatException)
        {
            noAnswerText.SetActive(true);
            Debug.Log("Error caught");
            return;
        }
        catch (ArgumentNullException)
        {
            noAnswerText.SetActive(true);
            Debug.Log("Error caught");
            return;
        }

        HelperModeSprite.SetActive(false);
        answerInput.SetActive(false);
        checkAnswerButton.SetActive(false);
        noAnswerText.SetActive(false);
        skipQuestionButton.SetActive(false);
        isFlipping = true;

        for (int i = 0; i < 10; i++)
        {
            leftHelpers[i].SetActive(false);
        }
        for (int i = 0; i < 10; i++)
        {
            rightHelpers[i].SetActive(false);
        }

        if (input == questionArray.questionArray[index].IntA + questionArray.questionArray[index].IntB)
        {
            questionArray.questionArray[index].ConsecutiveCorrect++;
            questionArray.questionArray[index].Difficulty--;
            questionArray.difficulties[index]--;

            ArmsDownHappy.SetActive(true);
            ArmsUpHappy.SetActive(true);

            if (questionArray.questionArray[index].ConsecutiveCorrect > questionArray.questionArray[index].Difficulty)
            {
                questionArray.questionArray[index].Difficulty = 0;
            }

            switch (random)
            {
                case 0:
                    questionText.text = "Correct!\n" + questionArray.questionArray[index].IntA + " + " + questionArray.questionArray[index].IntB + " equals\n\n"
                + (questionArray.questionArray[index].IntA + questionArray.questionArray[index].IntB);
                    break;
                case 1:
                    questionText.text = "Correct!\n" + questionArray.questionArray[index].IntB + " + " + questionArray.questionArray[index].IntA + " equals\n\n"
                + (questionArray.questionArray[index].IntA + questionArray.questionArray[index].IntB);
                    break;
            }
        }
        else
        {
            questionArray.questionArray[index].ConsecutiveCorrect = 0;
            questionArray.questionArray[index].Difficulty++;
            questionArray.difficulties[index]++;

            ArmsDownSad.SetActive(true);

            switch (random)
            {
                case 0:
                    questionText.text = "Incorrect :(\n" + questionArray.questionArray[index].IntA + " + " + questionArray.questionArray[index].IntB + " equals\n\n"
                + (questionArray.questionArray[index].IntA + questionArray.questionArray[index].IntB) + ", not " + input;
                    break;
                case 1:
                    questionText.text = "Incorrect :(\n" + questionArray.questionArray[index].IntB + " + " + questionArray.questionArray[index].IntA + " equals\n\n"
                + (questionArray.questionArray[index].IntA + questionArray.questionArray[index].IntB) + ", not " + input;
                    break;
            }
        }
    }

    //goes to the question page from the answer page when next question button is clicked. Generates new question, modifies fields
    public void NextQuestionClicked() {
        nextQuestionButton.SetActive(false);
        cardNum++;
        input = null;
        inputFieldText.text = "";
        timeCount = 0;
        Debug.Log("pressed2");

        ArmsDownSad.SetActive(false);
        ArmsUpHappy.SetActive(false);
        ArmsDownHappy.SetActive(false);

        if (questionArray.TotalDifficulty != 0)
        {
            index = questionArray.GetRandomQuestion();
            System.Random rnd = new System.Random();
            random = rnd.Next(0, 2);
            switch (random)
            {
                case 0:
                    questionText.text = "Question " + (cardNum + 1) + ":\nWhat is\n" + questionArray.questionArray[index].IntA + " + " + questionArray.questionArray[index].IntB;
                    break;
                case 1:
                    questionText.text = "Question " + (cardNum + 1) + ":\nWhat is\n" + questionArray.questionArray[index].IntB + " + " + questionArray.questionArray[index].IntA;
                    break;
            }

            answerInput.SetActive(true);
            checkAnswerButton.SetActive(true);
            skipQuestionButton.SetActive(true);

            if (assistantMode)
            {
                HelperModeSprite.SetActive(true);

                switch (random)
                {
                    case 0:
                        for (int i = 0; i <= questionArray.questionArray[index].IntA; i++)
                        {
                            leftHelpers[i].SetActive(true);
                        }
                        for (int i = 0; i <= questionArray.questionArray[index].IntB; i++)
                        {
                            rightHelpers[i].SetActive(true);
                        }
                        break;
                    case 1:
                        for (int i = 0; i <= questionArray.questionArray[index].IntB; i++)
                        {
                            leftHelpers[i].SetActive(true);
                        }
                        for (int i = 0; i <= questionArray.questionArray[index].IntA; i++)
                        {
                            rightHelpers[i].SetActive(true);
                        }
                        break;
                }
            }
        }
        else
        {
            questionText.text = "yay all done";
            nextQuestionButton.SetActive(false);
            celebration = true;

            ArmsUpHappy.SetActive(true);
            ArmsDownHappy.SetActive(true);
            goingUp = true;

        }
    }

    // Start is called before the first frame update
    void Start()
    {
        cardNum = 0;

        starterInput.SetActive(true);
        noAnswerText.SetActive(false);
        nextQuestionButton.SetActive(false);
        startScreen.SetActive(true);
        beginButton.SetActive(true);
        assistantModeButton.SetActive(true);

        questionBox.SetActive(false);
        answerInput.SetActive(false);
        checkAnswerButton.SetActive(false);
        skipQuestionButton.SetActive(false);

        ArmsDownHappy.SetActive(true);
        ArmsUpHappy.SetActive(true);
        ArmsDownSad.SetActive(false);
        HelperModeSprite.SetActive(false);

        leftHelpers.Add(leftHelper0);
        leftHelpers.Add(leftHelper1);
        leftHelpers.Add(leftHelper2);
        leftHelpers.Add(leftHelper3);
        leftHelpers.Add(leftHelper4);
        leftHelpers.Add(leftHelper5);
        leftHelpers.Add(leftHelper6);
        leftHelpers.Add(leftHelper7);
        leftHelpers.Add(leftHelper8);
        leftHelpers.Add(leftHelper9);

        rightHelpers.Add(rightHelper0);
        rightHelpers.Add(rightHelper1);
        rightHelpers.Add(rightHelper2);
        rightHelpers.Add(rightHelper3);
        rightHelpers.Add(rightHelper4);
        rightHelpers.Add(rightHelper5);
        rightHelpers.Add(rightHelper6);
        rightHelpers.Add(rightHelper7);
        rightHelpers.Add(rightHelper8);
        rightHelpers.Add(rightHelper9);

        for (int i = 0; i < 10; i++)
        {
            leftHelpers[i].SetActive(false);
        }
        for (int i = 0; i < 10; i++)
        {
            rightHelpers[i].SetActive(false);
        }

        restartButton.SetActive(false);

    celebration = false;

        distancePerTime = rShape.localScale.x / transitionTime;
    }

    // Update is called once per frame -- only used to "animate"
    void Update()
    {
        Vector3 vInitial = rShape.localScale;
        if (isFlipping)
        {
            Vector3 v = rShape.localScale;
            v.x += isShrinking * distancePerTime * Time.deltaTime;
            rShape.localScale = v;

            timeCount += Time.deltaTime;
            if ((timeCount >= transitionTime) && (isShrinking < 0))
            {
                timeCount = 0;
                isShrinking = 1;
            }

            else if ((timeCount >= transitionTime) && (isShrinking == 1))
            {
                rShape.localScale = vInitial;
                isFlipping = false;
                isShrinking = -1;
                nextQuestionButton.SetActive(true);
            }
        }

        if (celebration)
        {
            if (goingUp)
            {
                Vector3 movementUpRight = new Vector3(500, 50, 0);
                Vector3 movementUpLeft = new Vector3(-500, 50, 0);
                ArmsUpHappy.transform.Translate(movementUpRight);
                ArmsDownHappy.transform.Translate(movementUpLeft);
                goingUp = false;
            }
            else
            {
                Vector3 movementDownLeft = new Vector3(-500, -50, 0);
                Vector3 movementDownRight = new Vector3(500, -50, 0);
                ArmsUpHappy.transform.Translate(movementDownLeft);
                ArmsDownHappy.transform.Translate(movementDownRight);
                goingUp = true;
            }
        }
        
    }
    
}



