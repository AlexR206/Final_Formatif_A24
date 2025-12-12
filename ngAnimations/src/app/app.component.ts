import { transition, trigger, useAnimation } from '@angular/animations';
import { Component } from '@angular/core';
import { bounce, shakeX, tada } from 'ng-animate';

/*
  This component uses ng-animate prebuilt animations and Angular's animation triggers.
  Comments below explain why each part is present and how execution flows when buttons are clicked.
*/

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],

  // Define the Angular animation triggers used in the template.
  // Each trigger listens to ':increment' transitions. That means
  // whenever the bound numeric value increases (e.g. this.first++),
  // the associated animation plays.
  animations:[
    // Red square: shake animation for 2 seconds
    trigger('shake', [transition(':increment', useAnimation(shakeX, {
      params: { timing: 2.0 } // duration in seconds
    }))]),

    // Green square: bounce animation for 4 seconds
    trigger('bounce', [transition(':increment', useAnimation(bounce, {
      params: { timing: 4.0 }
    }))]),

    // Blue square: tada animation for 3 seconds
    trigger('tada', [transition(':increment', useAnimation(tada, {
      params: { timing: 3.0 }
    }))])
  ]
})
export class AppComponent {
  title = 'ngAnimations';

  // rotate boolean controls the presence of the CSS class 'rotate-left' on the orange square.
  // Setting rotate = true adds the class and starts the CSS animation;
  // resetting to false removes the class so the animation can be played again next click.
  rotate = false;

  // Numeric counters bound to the animation triggers in the template.
  // We increment these counters to trigger the animations via ':increment'.
  first = 0;   // drives the red "shake" animation
  second = 0;  // drives the green "bounce" animation
  third = 0;   // drives the blue "tada" animation

  constructor() { }

  /*
    rotateCube()
    - Called when the "Faire tourner" button is clicked.
    - Adds the rotate class by setting rotate = true.
    - After 2000ms (animation length), sets rotate = false so the class is removed.
    - Removing the class allows the same animation to be replayed on subsequent clicks.
    - We guard with an if to prevent re-entrancy while the animation is playing.
  */
  rotateCube() {
    if (this.rotate == false) {
      this.rotate = true;
      // The CSS animation is 2.0s; remove the class after that duration.
      setTimeout(() => { this.rotate = false; }, 2000);
    }
  }

  /*
    playNgAnimations(forever: boolean)
    - Orchestrates the required animation sequence:
      1) trigger red shake (2s)
      2) after 2s trigger green bounce (4s)
      3) 1s before bounce ends (i.e., 3s after bounce start), trigger blue tada (3s)
    - Implementation detail: we use nested setTimeout calls with durations that match each animation.
    - We use increments (first++, second++, third++) to trigger the Angular ':increment' transitions.
    - If forever == true, after the third animation is started we schedule a recursive call to
      playNgAnimations(true) so the sequence restarts after the full sequence finishes (loop).
  */
  playNgAnimations(forever: boolean) {
    // Start red shake immediately:
    // Incrementing 'first' changes the bound value and triggers the 'shake' animation.
    this.first++;

    // After 2000ms (2 sec) — when shake should complete — start bounce:
    setTimeout(() => {
      this.second++; // triggers 'bounce'

      // After 3000ms from the start of bounce (i.e. 1s before bounce ends),
      // start the blue 'tada' animation (which lasts 3s).
      // NOTE: bounce lasts 4s; starting tada after 3s makes it overlap the final second of bounce.
      setTimeout(() => {
        this.third++; // triggers 'tada'

        // If looping, schedule a new full run when the third animation is done.
        // The 3000ms delay here waits for 'tada' to finish before restarting.
        if (forever) {
          setTimeout(() => {
            // Recursive restart: plays the whole sequence again.
            this.playNgAnimations(true);
          }, 3000);
        }

      }, 3000); // time between starting bounce and starting tada
    }, 2000); // time to wait before starting bounce (after shake ends)
  }
}
