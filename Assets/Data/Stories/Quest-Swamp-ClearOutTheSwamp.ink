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
You look like you can help me. My name is Caley and I need your help. I need to get out of here. But there are these ...beasts in the swamp.  #Caley
-> theAsk

=== changeYourMind ===
You've come back! Please, can you clear out the swamp and check the pass to the forest. I can help you repair your ship. #Caley

+ [Alright, then - I'll do it.]
    -> accepted
+ [(Leave without replying)] 
    -> decline_ending

=== theAsk ===
~ knowsCaley = true
Maybe they are everywhere by now! So I'm hiding here. You look like you know how to solve problems like this. Can you help me?" #Caley

+ [Sure but I need your help too.]
    -> accepted
+ [Maybe another time.] 
    -> decline_ending


=== accepted ===
~ QS_Fukuiraptors_accepted = true
Sure, sure. I'll do whatever you want me to help you with. Just help me get out of here! #Caley

+ [I'll get going then.]
    -> ending


=== checkingIn ===
You alright? #Caley

+ [All good.]
    -> ending

=== decline_ending
But why? Alright then, come back if you change your mind, will you?" #Caley
-> DONE

=== completeQuest ===
~ QS_Fukuiraptors_completed = true
Oh, sorted out the swamp? You're alive too? This is fantastic. I didn't think you make it. Thank you so much! #Caley
    -> ending

=== ending
Alright then, see you later, I guess... #Caley

-> DONE

=== finalEnding ===
(If you see this, something whent is awfully wrong. Speak with your programmer about how you got here...) #Caley

-> END