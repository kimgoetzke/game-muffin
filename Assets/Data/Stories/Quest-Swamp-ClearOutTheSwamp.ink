INCLUDE GlobalVariables.ink

VAR unusedLocalString = "Unused"

{ knowsCaley:
 - false: -> hello
 - true: -> evaluateAcceptance
}


=== evaluateAcceptance ===
{ QS_Fukuiraptors_accepted:
 - false: -> changeYourMind
 - true: -> evaluateReady
}

=== evaluateReady ===
{ QS_Fukuiraptors_ready:
 - false: -> checkingIn
 - true: -> completeQuest
}

=== hello ===
You! How did you get here with THEM being out there?! Well, I guess this means you can help me. My name is Caley. I work for the Guild. Please help me get back to my team! I'm hiding here. These little ...beasts are out there. Our sensors didn't pick them up. #Caley
-> theAsk

=== changeYourMind ===
You've come back! Please, can you clear out these monsters and check the pass to the forest. I can help you repair your ship. #Caley

+ [Alright, then - I'll do it.]
    -> accepted
+ [(Leave without replying)] 
    -> decline_ending

=== theAsk ===
~ knowsCaley = true
Maybe they are everywhere by now. So I'm hiding here. I lost my equipment. You look like you know how to solve problems like this. Can you help me? You need to clear out these THINGS west of here and then to north west through that narrow pass, find Kalle and Saanvi, and bring them here." #Caley

+ [(Agree and explain your problem)]
    -> accepted
+ [Maybe another time.] 
    -> decline_ending


=== accepted ===
~ QS_Fukuiraptors_accepted = true
Sure, sure, sure. I can take you on my ship. We came down at the edge of the desert. Once you've found my team, well take you there. Don't think about trying this alone. Without our equipment you'll get lost. This place is like a maze.  We were just collecting data when we were attacked. #Caley

+ [I'll get going then.]
    -> ending


=== checkingIn ===
You alright? #Caley

+ [All good.]
    -> ending

=== decline_ending
But why? Please! Well, come back if you change your mind, will you? I don't know what to do." #Caley
-> DONE

=== completeQuest ===
~ QS_Fukuiraptors_completed = true
Oh, you did it? And you're alive too? This is fantastic. Wasn't sure you'd make it. The pass is blocked you say? I have explored this cave. Hang on. Come back in a minute. I think I found something that can help you. #Caley
    -> ending

=== ending
Alright then, see you later, I guess... #Caley

-> DONE

=== finalEnding ===
(If you see this, something whent is awfully wrong. Speak with your programmer about how you got here...) #Caley

-> END